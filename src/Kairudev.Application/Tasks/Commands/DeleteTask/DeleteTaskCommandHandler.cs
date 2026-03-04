using Kairudev.Application.Common;
using Kairudev.Domain.Tasks;

namespace Kairudev.Application.Tasks.Commands.DeleteTask;

public sealed class DeleteTaskCommandHandler
{
    private readonly ITaskRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public DeleteTaskCommandHandler(ITaskRepository repository, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<DeleteTaskResult> HandleAsync(
        DeleteTaskCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;

        var task = await _repository.GetByIdAsync(TaskId.From(command.TaskId), userId, cancellationToken);
        if (task is null)
            return DeleteTaskResult.NotFound();

        await _repository.DeleteAsync(task.Id, userId, cancellationToken);
        return DeleteTaskResult.Success();
    }
}
