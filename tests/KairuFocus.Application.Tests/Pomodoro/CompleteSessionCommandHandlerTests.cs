using KairuFocus.Application.Gamification.Commands.CreditSprintXp;
using KairuFocus.Application.Pomodoro.Commands.CompleteSession;
using KairuFocus.Application.Tests.Common;
using KairuFocus.Application.Tests.Gamification;
using KairuFocus.Application.Tests.Journal;
using KairuFocus.Domain.Identity;
using KairuFocus.Domain.Journal;
using KairuFocus.Domain.Pomodoro;
using Microsoft.Extensions.Logging.Abstractions;
using PomodoroErrors = KairuFocus.Domain.Pomodoro.DomainErrors;

namespace KairuFocus.Application.Tests.Pomodoro;

public sealed class CompleteSessionCommandHandlerTests
{
    private readonly FakePomodoroSessionRepository _sessionRepository = new();
    private readonly FakePomodoroSettingsRepository _settingsRepository = new();
    private readonly FakeJournalEntryRepository _journalRepository = new();
    private readonly FakeXpGainRepository _xpGainRepository = new();
    private readonly FakeMediator _fakeMediator;
    private readonly CompleteSessionCommandHandler _sut;

    public CompleteSessionCommandHandlerTests()
    {
        var creditSprintXpHandler = new CreditSprintXpCommandHandler(
            _xpGainRepository,
            _sessionRepository,
            NullLogger<CreditSprintXpCommandHandler>.Instance);
        _fakeMediator = new FakeMediator(_journalRepository, creditSprintXpHandler);
        _sut = new CompleteSessionCommandHandler(
            _sessionRepository,
            _settingsRepository,
            _fakeMediator,
            new FakeCurrentUserService(),
            NullLogger<CompleteSessionCommandHandler>.Instance);
    }

    private PomodoroSession AddActiveSession(PomodoroSessionType type, DateTime? startedAt = null)
    {
        var session = PomodoroSession.Create(type, 25, FakeCurrentUserService.TestUserId);
        session.Start(startedAt ?? DateTime.UtcNow);
        _sessionRepository.Sessions.Add(session);
        return session;
    }

    [Fact]
    public async Task Should_ReturnFailure_When_NoActiveSession()
    {
        var result = await _sut.Handle(new CompleteSessionCommand());

        Assert.False(result.IsSuccess);
        Assert.Equal(PomodoroErrors.Pomodoro.NoActiveSession, result.Error);
    }

    [Fact]
    public async Task Should_LogSprintCompleted_When_SprintSessionCompleted()
    {
        AddActiveSession(PomodoroSessionType.Sprint);

        var result = await _sut.Handle(new CompleteSessionCommand());

        Assert.True(result.IsSuccess);
        Assert.Single(_journalRepository.Entries);
        Assert.Equal(JournalEventType.SprintCompleted, _journalRepository.Entries[0].EventType);
    }

    [Fact]
    public async Task Should_LogBreakCompleted_When_ShortBreakSessionCompleted()
    {
        AddActiveSession(PomodoroSessionType.ShortBreak);

        var result = await _sut.Handle(new CompleteSessionCommand());

        Assert.True(result.IsSuccess);
        Assert.Single(_journalRepository.Entries);
        Assert.Equal(JournalEventType.BreakCompleted, _journalRepository.Entries[0].EventType);
    }

    [Fact]
    public async Task Should_SetSequence1_When_FirstBreakOfTheDay()
    {
        AddActiveSession(PomodoroSessionType.ShortBreak);

        await _sut.Handle(new CompleteSessionCommand());

        Assert.Equal(1, _journalRepository.Entries[0].Sequence);
    }

    [Fact]
    public async Task Should_SetSequence2_When_SecondBreakOfTheDay()
    {
        // première pause déjà enregistrée
        _journalRepository.Entries.Add(
            JournalEntry.Create(
                JournalEventType.BreakCompleted,
                Guid.NewGuid(),
                DateTime.UtcNow.AddHours(-1),
                FakeCurrentUserService.TestUserId,
                1));

        AddActiveSession(PomodoroSessionType.ShortBreak);

        await _sut.Handle(new CompleteSessionCommand());

        var breakEntry = _journalRepository.Entries.Last();
        Assert.Equal(2, breakEntry.Sequence);
    }

    [Fact]
    public async Task Should_NotSetSequence_When_SprintCompleted()
    {
        AddActiveSession(PomodoroSessionType.Sprint);

        await _sut.Handle(new CompleteSessionCommand());

        Assert.Null(_journalRepository.Entries[0].Sequence);
    }

    [Fact]
    public async Task Should_LogBreakCompleted_When_LongBreakSessionCompleted()
    {
        AddActiveSession(PomodoroSessionType.LongBreak);

        var result = await _sut.Handle(new CompleteSessionCommand());

        Assert.True(result.IsSuccess);
        Assert.Single(_journalRepository.Entries);
        Assert.Equal(JournalEventType.BreakCompleted, _journalRepository.Entries[0].EventType);
    }

    // ── XP credit (ADR-023) ────────────────────────────────────────────────

    [Fact]
    public async Task Should_CompleteSession_When_XpCreditThrows()
    {
        // Key rule from the product framing: a failing XP credit must NEVER
        // fail the session completion (log-only).
        AddActiveSession(PomodoroSessionType.Sprint, DateTime.UtcNow.AddMinutes(-25));
        _fakeMediator.ThrowOnCreditSprintXp = true;

        var result = await _sut.Handle(new CompleteSessionCommand());

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.XpAwarded);
        Assert.Equal(1, _sessionRepository.UpdateAsyncCallCount);
        Assert.Empty(_xpGainRepository.Gains);
    }

    [Fact]
    public async Task Should_CompleteSession_When_XpCreditFails()
    {
        // Non-throwing failure branch (e.g. SessionNotFound): the completion
        // must still succeed, with no XP awarded (log-only error handling).
        AddActiveSession(PomodoroSessionType.Sprint, DateTime.UtcNow.AddMinutes(-25));
        _fakeMediator.CreditSprintXpFailureError = "Session not found for XP credit.";

        var result = await _sut.Handle(new CompleteSessionCommand());

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.XpAwarded);
        Assert.Equal(1, _sessionRepository.UpdateAsyncCallCount);
        Assert.Empty(_xpGainRepository.Gains);
    }

    [Fact]
    public async Task Should_ReturnXpAwarded_When_SprintEligible()
    {
        var session = AddActiveSession(PomodoroSessionType.Sprint, DateTime.UtcNow.AddMinutes(-25));

        var result = await _sut.Handle(new CompleteSessionCommand());

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.XpAwarded);
        var gain = Assert.Single(_xpGainRepository.Gains);
        Assert.Equal(session.Id.Value, gain.SessionId);
    }

    [Fact]
    public async Task Should_ReturnZeroXpAwarded_When_BreakCompleted()
    {
        AddActiveSession(PomodoroSessionType.ShortBreak, DateTime.UtcNow.AddMinutes(-25));

        var result = await _sut.Handle(new CompleteSessionCommand());

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.XpAwarded);
        Assert.Empty(_xpGainRepository.Gains);
    }

    [Fact]
    public async Task Should_ReturnZeroXpAwarded_When_SprintUnder5Minutes()
    {
        AddActiveSession(PomodoroSessionType.Sprint, DateTime.UtcNow.AddMinutes(-4));

        var result = await _sut.Handle(new CompleteSessionCommand());

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.XpAwarded);
        Assert.Empty(_xpGainRepository.Gains);
    }
}
