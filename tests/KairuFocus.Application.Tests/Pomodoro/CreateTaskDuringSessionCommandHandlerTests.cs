using KairuFocus.Application.Pomodoro.Commands.CreateTaskDuringSession;
using KairuFocus.Application.Tests.Common;
using KairuFocus.Application.Tests.Tasks;
using KairuFocus.Domain.Pomodoro;
using Microsoft.Extensions.Logging.Abstractions;
using PomodoroErrors = KairuFocus.Domain.Pomodoro.DomainErrors;

namespace KairuFocus.Application.Tests.Pomodoro;

public sealed class CreateTaskDuringSessionCommandHandlerTests
{
    private readonly FakePomodoroSessionRepository _sessionRepository = new();
    private readonly FakeTaskRepository _taskRepository = new();
    private readonly CreateTaskDuringSessionCommandHandler _sut;

    public CreateTaskDuringSessionCommandHandlerTests()
    {
        _sut = new CreateTaskDuringSessionCommandHandler(
            _sessionRepository,
            _taskRepository,
            new FakeCurrentUserService(),
            NullLogger<CreateTaskDuringSessionCommandHandler>.Instance);
    }

    [Fact]
    public async Task Should_NotCreateTask_When_SessionIsShortBreak()
    {
        var session = PomodoroSession.Create(PomodoroSessionType.ShortBreak, 5, FakeCurrentUserService.TestUserId);
        session.Start(DateTime.UtcNow);
        _sessionRepository.Sessions.Add(session);

        var result = await _sut.Handle(new CreateTaskDuringSessionCommand("My task", null));

        Assert.False(result.IsSuccess);
        Assert.Equal(PomodoroErrors.Pomodoro.TaskLinkingNotAllowedForBreak, result.Error);
        Assert.Empty(_taskRepository.Tasks);
    }

    [Fact]
    public async Task Should_NotCreateTask_When_SessionIsLongBreak()
    {
        var session = PomodoroSession.Create(PomodoroSessionType.LongBreak, 15, FakeCurrentUserService.TestUserId);
        session.Start(DateTime.UtcNow);
        _sessionRepository.Sessions.Add(session);

        var result = await _sut.Handle(new CreateTaskDuringSessionCommand("My task", null));

        Assert.False(result.IsSuccess);
        Assert.Equal(PomodoroErrors.Pomodoro.TaskLinkingNotAllowedForBreak, result.Error);
        Assert.Empty(_taskRepository.Tasks);
    }

    [Fact]
    public async Task Should_CreateAndLinkTask_When_SessionIsSprint()
    {
        var session = PomodoroSession.Create(PomodoroSessionType.Sprint, 25, FakeCurrentUserService.TestUserId);
        session.Start(DateTime.UtcNow);
        _sessionRepository.Sessions.Add(session);

        var result = await _sut.Handle(new CreateTaskDuringSessionCommand("My task", null));

        Assert.True(result.IsSuccess);
        Assert.Single(_taskRepository.Tasks);
        Assert.Single(session.LinkedTaskIds);
    }
}
