using KairuFocus.Application.Pomodoro.Commands.InterruptSession;
using KairuFocus.Application.Tests.Common;
using KairuFocus.Application.Tests.Journal;
using KairuFocus.Domain.Identity;
using KairuFocus.Domain.Journal;
using KairuFocus.Domain.Pomodoro;
using Microsoft.Extensions.Logging.Abstractions;
using PomodoroErrors = KairuFocus.Domain.Pomodoro.DomainErrors;

namespace KairuFocus.Application.Tests.Pomodoro;

public sealed class InterruptSessionCommandHandlerTests
{
    private readonly FakePomodoroSessionRepository _sessionRepository = new();
    private readonly FakeJournalEntryRepository _journalRepository = new();
    private readonly InterruptSessionCommandHandler _sut;

    public InterruptSessionCommandHandlerTests()
    {
        var fakeMediator = new FakeMediator(_journalRepository);
        _sut = new InterruptSessionCommandHandler(
            _sessionRepository,
            fakeMediator,
            new FakeCurrentUserService(),
            NullLogger<InterruptSessionCommandHandler>.Instance);
    }

    private PomodoroSession AddActiveSession(PomodoroSessionType type)
    {
        var session = PomodoroSession.Create(type, 25, FakeCurrentUserService.TestUserId);
        session.Start(DateTime.UtcNow);
        _sessionRepository.Sessions.Add(session);
        return session;
    }

    [Fact]
    public async Task Should_ReturnFailure_When_NoActiveSession()
    {
        var result = await _sut.Handle(new InterruptSessionCommand());

        Assert.False(result.IsSuccess);
        Assert.Equal(PomodoroErrors.Pomodoro.NoActiveSession, result.Error);
    }

    [Fact]
    public async Task Should_LogSprintInterrupted_When_SprintSessionInterrupted()
    {
        AddActiveSession(PomodoroSessionType.Sprint);

        var result = await _sut.Handle(new InterruptSessionCommand());

        Assert.True(result.IsSuccess);
        Assert.Single(_journalRepository.Entries);
        Assert.Equal(JournalEventType.SprintInterrupted, _journalRepository.Entries[0].EventType);
    }

    [Fact]
    public async Task Should_LogBreakInterrupted_When_ShortBreakSessionInterrupted()
    {
        AddActiveSession(PomodoroSessionType.ShortBreak);

        var result = await _sut.Handle(new InterruptSessionCommand());

        Assert.True(result.IsSuccess);
        Assert.Single(_journalRepository.Entries);
        Assert.Equal(JournalEventType.BreakInterrupted, _journalRepository.Entries[0].EventType);
    }

    [Fact]
    public async Task Should_LogBreakInterrupted_When_LongBreakSessionInterrupted()
    {
        AddActiveSession(PomodoroSessionType.LongBreak);

        var result = await _sut.Handle(new InterruptSessionCommand());

        Assert.True(result.IsSuccess);
        Assert.Single(_journalRepository.Entries);
        Assert.Equal(JournalEventType.BreakInterrupted, _journalRepository.Entries[0].EventType);
    }
}
