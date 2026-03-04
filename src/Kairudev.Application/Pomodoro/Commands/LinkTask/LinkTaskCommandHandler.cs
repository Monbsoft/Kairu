using Kairudev.Application.Common;
using Kairudev.Domain.Pomodoro;
using Kairudev.Domain.Tasks;

namespace Kairudev.Application.Pomodoro.Commands.LinkTask;

public sealed class LinkTaskCommandHandler
{
    private readonly IPomodoroSessionRepository _sessionRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ICurrentUserService _currentUserService;

    public LinkTaskCommandHandler(
        IPomodoroSessionRepository sessionRepository,
        ITaskRepository taskRepository,
        ICurrentUserService currentUserService)
    {
        _sessionRepository = sessionRepository;
        _taskRepository = taskRepository;
        _currentUserService = currentUserService;
    }

    public async Task<LinkTaskResult> HandleAsync(
        LinkTaskCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;

        var session = await _sessionRepository.GetActiveAsync(userId, cancellationToken);
        if (session is null)
            return LinkTaskResult.Failure("No active session");

        var task = await _taskRepository.GetByIdAsync(TaskId.From(command.TaskId), userId, cancellationToken);
        if (task is null)
            return LinkTaskResult.NotFound();

        var result = session.LinkTask(task.Id);
        if (result.IsFailure)
            return LinkTaskResult.Failure(result.Error);

        await _sessionRepository.UpdateAsync(session, cancellationToken);
        return LinkTaskResult.Success();
    }
}
