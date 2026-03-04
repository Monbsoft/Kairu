using Kairudev.Application.Journal.Commands.CreateEntry;
using Kairudev.Application.Pomodoro.Commands.InterruptSession;
using Kairudev.Application.Tests.Common;
using Kairudev.Application.Tests.Journal;
using Kairudev.Domain.Identity;
using Kairudev.Domain.Journal;
using Kairudev.Domain.Pomodoro;

namespace Kairudev.Application.Tests.Pomodoro;

public sealed class InterruptSessionCommandHandlerTests
{
    private readonly FakePomodoroSessionRepository _sessionRepository = new();
    private readonly FakeJournalEntryRepository _journalRepository = new();
    private readonly InterruptSessionCommandHandler _sut;

    public InterruptSessionCommandHandlerTests()
    {
        var journalHandler = new CreateEntryCommandHandler(_journalRepository);
        _sut = new InterruptSessionCommandHandler(
            _sessionRepository,
            journalHandler,
            new FakeCurrentUserService());
    }

    private PomodoroSession AddActiveSession(PomodoroSessionType type)
    {
        var session = PomodoroSession.Create(type, 25, UserId.From("test-github-id-123"));
        session.Start(DateTime.UtcNow);
        _sessionRepository.Sessions.Add(session);
        return session;
    }

    [Fact]
    public async Task Should_ReturnFailure_When_NoActiveSession()
    {
        var result = await _sut.HandleAsync(new InterruptSessionCommand());

        Assert.False(result.IsSuccess);
        Assert.Equal("No active session", result.Error);
    }

    [Fact]
    public async Task Should_LogSprintInterrupted_When_SprintSessionInterrupted()
    {
        AddActiveSession(PomodoroSessionType.Sprint);

        var result = await _sut.HandleAsync(new InterruptSessionCommand());

        Assert.True(result.IsSuccess);
        Assert.Single(_journalRepository.Entries);
        Assert.Equal(JournalEventType.SprintInterrupted, _journalRepository.Entries[0].EventType);
    }

    [Fact]
    public async Task Should_LogBreakInterrupted_When_ShortBreakSessionInterrupted()
    {
        AddActiveSession(PomodoroSessionType.ShortBreak);

        var result = await _sut.HandleAsync(new InterruptSessionCommand());

        Assert.True(result.IsSuccess);
        Assert.Single(_journalRepository.Entries);
        Assert.Equal(JournalEventType.BreakInterrupted, _journalRepository.Entries[0].EventType);
    }

    [Fact]
    public async Task Should_LogBreakInterrupted_When_LongBreakSessionInterrupted()
    {
        AddActiveSession(PomodoroSessionType.LongBreak);

        var result = await _sut.HandleAsync(new InterruptSessionCommand());

        Assert.True(result.IsSuccess);
        Assert.Single(_journalRepository.Entries);
        Assert.Equal(JournalEventType.BreakInterrupted, _journalRepository.Entries[0].EventType);
    }
}
