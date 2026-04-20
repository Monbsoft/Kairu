using KairuFocus.Application.Pomodoro.Commands.UnlinkTask;
using KairuFocus.Application.Tests.Common;
using KairuFocus.Domain.Pomodoro;
using KairuFocus.Domain.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using PomodoroErrors = KairuFocus.Domain.Pomodoro.DomainErrors;

namespace KairuFocus.Application.Tests.Pomodoro;

public sealed class UnlinkTaskCommandHandlerTests
{
    private readonly FakePomodoroSessionRepository _sessionRepository = new();
    private readonly UnlinkTaskCommandHandler _sut;

    public UnlinkTaskCommandHandlerTests()
    {
        _sut = new UnlinkTaskCommandHandler(
            _sessionRepository,
            new FakeCurrentUserService(),
            NullLogger<UnlinkTaskCommandHandler>.Instance);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_NoActiveSession()
    {
        var result = await _sut.Handle(new UnlinkTaskCommand(Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.Equal(PomodoroErrors.Pomodoro.NoActiveSession, result.Error);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TaskNotLinked()
    {
        var session = PomodoroSession.Create(PomodoroSessionType.Sprint, 25, FakeCurrentUserService.TestUserId);
        session.Start(DateTime.UtcNow);
        _sessionRepository.Sessions.Add(session);

        var result = await _sut.Handle(new UnlinkTaskCommand(Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.True(result.IsNotFound);
    }

    [Fact]
    public async Task Should_ReturnSuccess_When_TaskIsLinked()
    {
        var session = PomodoroSession.Create(PomodoroSessionType.Sprint, 25, FakeCurrentUserService.TestUserId);
        session.Start(DateTime.UtcNow);
        var taskId = TaskId.New();
        session.LinkTask(taskId);
        _sessionRepository.Sessions.Add(session);

        var result = await _sut.Handle(new UnlinkTaskCommand(taskId.Value));

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(taskId, session.LinkedTaskIds);
        Assert.Equal(1, _sessionRepository.UpdateAsyncCallCount);
    }
}
