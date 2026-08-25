using KairuFocus.Application.Gamification.Commands.CreditSprintXp;
using KairuFocus.Application.Tests.Common;
using KairuFocus.Application.Tests.Pomodoro;
using KairuFocus.Domain.Pomodoro;
using Microsoft.Extensions.Logging.Abstractions;
using GamificationErrors = KairuFocus.Domain.Gamification.DomainErrors;

namespace KairuFocus.Application.Tests.Gamification;

public sealed class CreditSprintXpCommandHandlerTests
{
    private static readonly DateTime StartedAt = new(2026, 7, 15, 10, 0, 0, DateTimeKind.Utc);

    private readonly FakeXpGainRepository _xpGainRepository = new();
    private readonly FakePomodoroSessionRepository _sessionRepository = new();
    private readonly CreditSprintXpCommandHandler _sut;

    public CreditSprintXpCommandHandlerTests()
    {
        _sut = new CreditSprintXpCommandHandler(
            _xpGainRepository,
            _sessionRepository,
            NullLogger<CreditSprintXpCommandHandler>.Instance);
    }

    private PomodoroSession AddSession(PomodoroSessionType type, int durationMinutes, bool interrupt = false)
    {
        var session = PomodoroSession.Create(type, 25, FakeCurrentUserService.TestUserId);
        session.Start(StartedAt);
        if (interrupt)
            session.Interrupt(StartedAt.AddMinutes(durationMinutes));
        else
            session.Complete(StartedAt.AddMinutes(durationMinutes));
        _sessionRepository.Sessions.Add(session);
        return session;
    }

    [Fact]
    public async Task Should_Credit10Xp_When_SprintEligible()
    {
        var session = AddSession(PomodoroSessionType.Sprint, 25);

        var result = await _sut.Handle(new CreditSprintXpCommand(session.Id.Value));

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.XpAwarded);
        var gain = Assert.Single(_xpGainRepository.Gains);
        Assert.Equal(session.OwnerId, gain.OwnerId);
        Assert.Equal(session.Id.Value, gain.SessionId);
        Assert.Equal(session.EndedAt!.Value, gain.EarnedAtUtc);
    }

    [Fact]
    public async Task Should_ReturnZeroWithoutAdding_When_SessionAlreadyCredited()
    {
        var session = AddSession(PomodoroSessionType.Sprint, 25);
        await _sut.Handle(new CreditSprintXpCommand(session.Id.Value));

        // Idempotence: re-dispatching the same command must be a no-op.
        var result = await _sut.Handle(new CreditSprintXpCommand(session.Id.Value));

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.XpAwarded);
        Assert.Equal(1, _xpGainRepository.AddAsyncCallCount);
        Assert.Single(_xpGainRepository.Gains);
    }

    [Fact]
    public async Task Should_ReturnZero_When_SessionIsBreak()
    {
        var session = AddSession(PomodoroSessionType.ShortBreak, 5);

        var result = await _sut.Handle(new CreditSprintXpCommand(session.Id.Value));

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.XpAwarded);
        Assert.Empty(_xpGainRepository.Gains);
    }

    [Fact]
    public async Task Should_ReturnZero_When_SprintUnder5Minutes()
    {
        var session = AddSession(PomodoroSessionType.Sprint, 4);

        var result = await _sut.Handle(new CreditSprintXpCommand(session.Id.Value));

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.XpAwarded);
        Assert.Empty(_xpGainRepository.Gains);
    }

    [Fact]
    public async Task Should_ReturnZero_When_SessionInterrupted()
    {
        var session = AddSession(PomodoroSessionType.Sprint, 25, interrupt: true);

        var result = await _sut.Handle(new CreditSprintXpCommand(session.Id.Value));

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.XpAwarded);
        Assert.Empty(_xpGainRepository.Gains);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_SessionNotFound()
    {
        var result = await _sut.Handle(new CreditSprintXpCommand(Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.Equal(GamificationErrors.Gamification.SessionNotFound, result.Error);
        Assert.Empty(_xpGainRepository.Gains);
    }
}
