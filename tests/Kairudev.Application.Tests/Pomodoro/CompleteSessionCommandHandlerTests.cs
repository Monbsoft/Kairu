using Kairudev.Application.Journal.Commands.CreateEntry;
using Kairudev.Application.Pomodoro.Commands.CompleteSession;
using Kairudev.Application.Tests.Journal;
using Kairudev.Domain.Journal;
using Kairudev.Domain.Pomodoro;

namespace Kairudev.Application.Tests.Pomodoro;

public sealed class CompleteSessionCommandHandlerTests
{
    private readonly FakePomodoroSessionRepository _sessionRepository = new();
    private readonly FakePomodoroSettingsRepository _settingsRepository = new();
    private readonly FakeJournalEntryRepository _journalRepository = new();
    private readonly CompleteSessionCommandHandler _sut;

    public CompleteSessionCommandHandlerTests()
    {
        var journalHandler = new CreateEntryCommandHandler(_journalRepository);
        _sut = new CompleteSessionCommandHandler(_sessionRepository, _settingsRepository, journalHandler);
    }

    private PomodoroSession AddActiveSession(PomodoroSessionType type)
    {
        var session = PomodoroSession.Create(type, 25);
        session.Start(DateTime.UtcNow);
        _sessionRepository.Sessions.Add(session);
        return session;
    }

    [Fact]
    public async Task Should_ReturnFailure_When_NoActiveSession()
    {
        var result = await _sut.HandleAsync(new CompleteSessionCommand());

        Assert.False(result.IsSuccess);
        Assert.Equal("No active session", result.Error);
    }

    [Fact]
    public async Task Should_LogSprintCompleted_When_SprintSessionCompleted()
    {
        AddActiveSession(PomodoroSessionType.Sprint);

        var result = await _sut.HandleAsync(new CompleteSessionCommand());

        Assert.True(result.IsSuccess);
        Assert.Single(_journalRepository.Entries);
        Assert.Equal(JournalEventType.SprintCompleted, _journalRepository.Entries[0].EventType);
    }

    [Fact]
    public async Task Should_LogBreakCompleted_When_ShortBreakSessionCompleted()
    {
        AddActiveSession(PomodoroSessionType.ShortBreak);

        var result = await _sut.HandleAsync(new CompleteSessionCommand());

        Assert.True(result.IsSuccess);
        Assert.Single(_journalRepository.Entries);
        Assert.Equal(JournalEventType.BreakCompleted, _journalRepository.Entries[0].EventType);
    }

    [Fact]
    public async Task Should_LogBreakCompleted_When_LongBreakSessionCompleted()
    {
        AddActiveSession(PomodoroSessionType.LongBreak);

        var result = await _sut.HandleAsync(new CompleteSessionCommand());

        Assert.True(result.IsSuccess);
        Assert.Single(_journalRepository.Entries);
        Assert.Equal(JournalEventType.BreakCompleted, _journalRepository.Entries[0].EventType);
    }
}
