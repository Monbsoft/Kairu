using KairuFocus.Application.Pomodoro.Commands.LinkTask;
using KairuFocus.Application.Tests.Common;
using KairuFocus.Application.Tests.Tasks;
using KairuFocus.Domain.Pomodoro;
using KairuFocus.Domain.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using PomodoroErrors = KairuFocus.Domain.Pomodoro.DomainErrors;

namespace KairuFocus.Application.Tests.Pomodoro;

public sealed class LinkTaskCommandHandlerTests
{
    private readonly FakePomodoroSessionRepository _sessionRepository = new();
    private readonly FakeTaskRepository _taskRepository = new();
    private readonly LinkTaskCommandHandler _sut;

    public LinkTaskCommandHandlerTests()
    {
        _sut = new LinkTaskCommandHandler(
            _sessionRepository,
            _taskRepository,
            new FakeCurrentUserService(),
            NullLogger<LinkTaskCommandHandler>.Instance);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_SessionIsShortBreak()
    {
        var session = PomodoroSession.Create(PomodoroSessionType.ShortBreak, 5, FakeCurrentUserService.TestUserId);
        session.Start(DateTime.UtcNow);
        _sessionRepository.Sessions.Add(session);

        var task = DeveloperTask.Create(
            TaskTitle.Create("Write report").Value,
            null,
            DateTime.UtcNow,
            FakeCurrentUserService.TestUserId);
        _taskRepository.Tasks.Add(task);

        var result = await _sut.Handle(new LinkTaskCommand(task.Id.Value));

        Assert.False(result.IsSuccess);
        Assert.Equal(PomodoroErrors.Pomodoro.TaskLinkingNotAllowedForBreak, result.Error);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_SessionIsLongBreak()
    {
        var session = PomodoroSession.Create(PomodoroSessionType.LongBreak, 15, FakeCurrentUserService.TestUserId);
        session.Start(DateTime.UtcNow);
        _sessionRepository.Sessions.Add(session);

        var task = DeveloperTask.Create(
            TaskTitle.Create("Write report").Value,
            null,
            DateTime.UtcNow,
            FakeCurrentUserService.TestUserId);
        _taskRepository.Tasks.Add(task);

        var result = await _sut.Handle(new LinkTaskCommand(task.Id.Value));

        Assert.False(result.IsSuccess);
        Assert.Equal(PomodoroErrors.Pomodoro.TaskLinkingNotAllowedForBreak, result.Error);
    }

    [Fact]
    public async Task Should_ReturnSuccess_When_SessionIsSprint()
    {
        var session = PomodoroSession.Create(PomodoroSessionType.Sprint, 25, FakeCurrentUserService.TestUserId);
        session.Start(DateTime.UtcNow);
        _sessionRepository.Sessions.Add(session);

        var task = DeveloperTask.Create(
            TaskTitle.Create("Write report").Value,
            null,
            DateTime.UtcNow,
            FakeCurrentUserService.TestUserId);
        _taskRepository.Tasks.Add(task);

        var result = await _sut.Handle(new LinkTaskCommand(task.Id.Value));

        Assert.True(result.IsSuccess);
        Assert.Contains(task.Id, session.LinkedTaskIds);
        Assert.Equal(1, _sessionRepository.UpdateAsyncCallCount);
    }
}
