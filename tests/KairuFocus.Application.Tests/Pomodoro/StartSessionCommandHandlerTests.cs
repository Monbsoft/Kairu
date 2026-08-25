using KairuFocus.Application.Pomodoro.Commands.StartSession;
using KairuFocus.Application.Tests.Common;
using KairuFocus.Application.Tests.Journal;
using KairuFocus.Domain.Identity;
using KairuFocus.Domain.Journal;
using KairuFocus.Domain.Pomodoro;
using Microsoft.Extensions.Logging.Abstractions;
using PomodoroErrors = KairuFocus.Domain.Pomodoro.DomainErrors;

namespace KairuFocus.Application.Tests.Pomodoro;

public sealed class StartSessionCommandHandlerTests
{
    private readonly FakePomodoroSessionRepository _sessionRepository = new();
    private readonly FakePomodoroSettingsRepository _settingsRepository = new();
    private readonly FakeJournalEntryRepository _journalRepository = new();
    private readonly FakeMediator _mediator;
    private readonly StartSessionCommandHandler _sut;

    public StartSessionCommandHandlerTests()
    {
        _mediator = new FakeMediator(_journalRepository);
        _sut = new StartSessionCommandHandler(
            _sessionRepository,
            _settingsRepository,
            _mediator,
            new FakeCurrentUserService(),
            NullLogger<StartSessionCommandHandler>.Instance);
    }

    private PomodoroSession GivenActiveSession(
        PomodoroSessionType type,
        DateTime startedAt,
        int plannedDurationMinutes = 25)
    {
        var session = CreateActiveSession(type, startedAt, plannedDurationMinutes);
        _sessionRepository.Sessions.Add(session);
        return session;
    }

    private static PomodoroSession CreateActiveSession(
        PomodoroSessionType type,
        DateTime startedAt,
        int plannedDurationMinutes = 25)
    {
        var session = PomodoroSession.Create(type, plannedDurationMinutes, FakeCurrentUserService.TestUserId);
        session.Start(startedAt);
        return session;
    }

    [Fact]
    public async Task Should_ReturnTheExistingSession_When_TheSameStartIsSubmittedTwiceInARow()
    {
        // Double submission of the same click: the second call must be idempotent.
        var existing = GivenActiveSession(PomodoroSessionType.Sprint, DateTime.UtcNow);

        var result = await _sut.Handle(new StartSessionCommand("Sprint"));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Session);
        Assert.Equal(existing.Id.Value, result.Session!.Id);
        Assert.Single(_sessionRepository.Sessions);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_AnActiveSessionOfAnotherTypeExists()
    {
        GivenActiveSession(PomodoroSessionType.Sprint, DateTime.UtcNow);

        var result = await _sut.Handle(new StartSessionCommand("ShortBreak"));

        Assert.False(result.IsSuccess);
        Assert.Equal(PomodoroErrors.Pomodoro.SessionAlreadyActive, result.Error);
        Assert.Single(_sessionRepository.Sessions);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_TheActiveSessionIsOlderThanTheIdempotencyWindow()
    {
        GivenActiveSession(PomodoroSessionType.Sprint, DateTime.UtcNow.AddMinutes(-10));

        var result = await _sut.Handle(new StartSessionCommand("Sprint"));

        Assert.False(result.IsSuccess);
        Assert.Equal(PomodoroErrors.Pomodoro.SessionAlreadyActive, result.Error);
        Assert.Single(_sessionRepository.Sessions);
    }

    [Fact]
    public async Task Should_ReturnSuccess_When_TheJournalEntryDispatchThrows()
    {
        // ADR-023: the session is already persisted, a failing side effect must not fail the start.
        _mediator.ThrowOnCreateEntry = true;

        var result = await _sut.Handle(new StartSessionCommand("Sprint"));

        Assert.True(result.IsSuccess);
        Assert.Single(_sessionRepository.Sessions);
        Assert.Empty(_journalRepository.Entries);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_TheActiveSessionStartedInTheFuture()
    {
        // Clock drift between replicas: a StartedAt in the future must never be read as a
        // re-submission of the current click.
        GivenActiveSession(PomodoroSessionType.Sprint, DateTime.UtcNow.AddMinutes(10));

        var result = await _sut.Handle(new StartSessionCommand("Sprint"));

        Assert.False(result.IsSuccess);
        Assert.Equal(PomodoroErrors.Pomodoro.SessionAlreadyActive, result.Error);
        Assert.Single(_sessionRepository.Sessions);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_AFreeSprintIsActiveAndARegularSprintIsRequested()
    {
        // A free sprint (planned duration 0) is not the same start as a regular sprint:
        // returning it would make the client run a 0-minute timer and close it instantly.
        GivenActiveSession(PomodoroSessionType.Sprint, DateTime.UtcNow.AddSeconds(-2), plannedDurationMinutes: 0);

        var result = await _sut.Handle(new StartSessionCommand("Sprint"));

        Assert.False(result.IsSuccess);
        Assert.Equal(PomodoroErrors.Pomodoro.SessionAlreadyActive, result.Error);
        Assert.Single(_sessionRepository.Sessions);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_ARegularSprintIsActiveAndAFreeSprintIsRequested()
    {
        GivenActiveSession(PomodoroSessionType.Sprint, DateTime.UtcNow.AddSeconds(-2));

        var result = await _sut.Handle(new StartSessionCommand(null, IsFreeSession: true));

        Assert.False(result.IsSuccess);
        Assert.Equal(PomodoroErrors.Pomodoro.SessionAlreadyActive, result.Error);
        Assert.Single(_sessionRepository.Sessions);
    }

    [Fact]
    public async Task Should_ReturnTheExistingFreeSprint_When_TheSameFreeStartIsSubmittedTwiceInARow()
    {
        var existing = GivenActiveSession(
            PomodoroSessionType.Sprint, DateTime.UtcNow.AddSeconds(-2), plannedDurationMinutes: 0);

        var result = await _sut.Handle(new StartSessionCommand(null, IsFreeSession: true));

        Assert.True(result.IsSuccess);
        Assert.Equal(existing.Id.Value, result.Session!.Id);
        Assert.Single(_sessionRepository.Sessions);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_TheRepositoryRejectsAConcurrentInsert()
    {
        // The unique index rejected the insert and no compatible session can be read back:
        // the winning row was closed in between.
        PomodoroSession? winner = null;
        _sessionRepository.OnTryAdd = () =>
        {
            winner = CreateActiveSession(PomodoroSessionType.Sprint, DateTime.UtcNow);
            _sessionRepository.Sessions.Add(winner);
        };
        _sessionRepository.OnInsertRejected = () => winner!.Interrupt(DateTime.UtcNow);

        var result = await _sut.Handle(new StartSessionCommand("Sprint"));

        Assert.False(result.IsSuccess);
        Assert.Equal(PomodoroErrors.Pomodoro.ConcurrentSessionStart, result.Error);
    }

    [Fact]
    public async Task Should_ReturnTheWinningSession_When_AConcurrentStartOfTheSameTypeWonTheRace()
    {
        // The insert is rejected by the "one active session per user" invariant while the
        // concurrent request that won the race started the very same session.
        PomodoroSession? winner = null;
        _sessionRepository.OnTryAdd = () =>
        {
            winner = CreateActiveSession(PomodoroSessionType.Sprint, DateTime.UtcNow);
            _sessionRepository.Sessions.Add(winner);
        };

        var result = await _sut.Handle(new StartSessionCommand("Sprint"));

        Assert.True(result.IsSuccess);
        Assert.NotNull(winner);
        Assert.Equal(winner!.Id.Value, result.Session!.Id);
        Assert.Single(_sessionRepository.Sessions);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_AConcurrentStartOfAnotherTypeWonTheRace()
    {
        _sessionRepository.OnTryAdd = () =>
            _sessionRepository.Sessions.Add(
                CreateActiveSession(PomodoroSessionType.ShortBreak, DateTime.UtcNow));

        var result = await _sut.Handle(new StartSessionCommand("Sprint"));

        Assert.False(result.IsSuccess);
        Assert.Equal(PomodoroErrors.Pomodoro.SessionAlreadyActive, result.Error);
        Assert.Single(_sessionRepository.Sessions);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_AFreeSprintWonTheRaceAndARegularSprintIsRequested()
    {
        _sessionRepository.OnTryAdd = () =>
            _sessionRepository.Sessions.Add(
                CreateActiveSession(PomodoroSessionType.Sprint, DateTime.UtcNow, plannedDurationMinutes: 0));

        var result = await _sut.Handle(new StartSessionCommand("Sprint"));

        Assert.False(result.IsSuccess);
        Assert.Equal(PomodoroErrors.Pomodoro.SessionAlreadyActive, result.Error);
        Assert.Single(_sessionRepository.Sessions);
    }

    [Fact]
    public async Task Should_StartASprint_When_TheSessionTypeIsNull()
    {
        var result = await _sut.Handle(new StartSessionCommand(null));

        Assert.True(result.IsSuccess);
        Assert.Equal(nameof(PomodoroSessionType.Sprint), result.Session!.SessionType);
        Assert.Equal(25, result.Session.PlannedDurationMinutes);
    }

    [Fact]
    public async Task Should_StartASprint_When_TheSessionTypeIsUnknown()
    {
        var result = await _sut.Handle(new StartSessionCommand("NotASessionType"));

        Assert.True(result.IsSuccess);
        Assert.Equal(nameof(PomodoroSessionType.Sprint), result.Session!.SessionType);
    }

    [Fact]
    public async Task Should_StartAFreeSprintWithoutPlannedDuration_When_IsFreeSessionIsSet()
    {
        var result = await _sut.Handle(new StartSessionCommand(null, IsFreeSession: true, JournalComment: "refacto"));

        Assert.True(result.IsSuccess);
        Assert.Equal(nameof(PomodoroSessionType.Sprint), result.Session!.SessionType);
        Assert.Equal(0, result.Session.PlannedDurationMinutes);
        var stored = Assert.Single(_sessionRepository.Sessions);
        Assert.Equal("refacto", stored.JournalComment);
    }

    [Fact]
    public async Task Should_LogSprintStarted_When_SprintSessionStarted()
    {
        var result = await _sut.Handle(new StartSessionCommand("Sprint"));

        Assert.True(result.IsSuccess);
        Assert.Single(_journalRepository.Entries);
        Assert.Equal(JournalEventType.SprintStarted, _journalRepository.Entries[0].EventType);
    }

    [Fact]
    public async Task Should_LogBreakStarted_When_ShortBreakSessionStarted()
    {
        var result = await _sut.Handle(new StartSessionCommand("ShortBreak"));

        Assert.True(result.IsSuccess);
        Assert.Single(_journalRepository.Entries);
        Assert.Equal(JournalEventType.BreakStarted, _journalRepository.Entries[0].EventType);
    }

    [Fact]
    public async Task Should_LogBreakStarted_When_LongBreakSessionStarted()
    {
        var result = await _sut.Handle(new StartSessionCommand("LongBreak"));

        Assert.True(result.IsSuccess);
        Assert.Single(_journalRepository.Entries);
        Assert.Equal(JournalEventType.BreakStarted, _journalRepository.Entries[0].EventType);
    }
}
