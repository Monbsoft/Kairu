using Kairudev.Application.Common;
using Kairudev.Application.Tasks.Common;
using Kairudev.Domain.Tasks;

namespace Kairudev.Application.Tasks.Commands.UpdateTask;

public sealed class UpdateTaskCommandHandler
{
    private readonly ITaskRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public UpdateTaskCommandHandler(ITaskRepository repository, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<UpdateTaskResult> HandleAsync(
        UpdateTaskCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;

        var task = await _repository.GetByIdAsync(TaskId.From(command.TaskId), userId, cancellationToken);
        if (task is null)
            return UpdateTaskResult.NotFound();

        var titleResult = TaskTitle.Create(command.Title);
        if (titleResult.IsFailure)
            return UpdateTaskResult.Failure(titleResult.Error);

        var descriptionResult = TaskDescription.Create(command.Description);
        if (descriptionResult.IsFailure)
            return UpdateTaskResult.Failure(descriptionResult.Error);

        task.UpdateDetails(titleResult.Value, descriptionResult.Value);

        await _repository.UpdateAsync(task, cancellationToken);
        return UpdateTaskResult.Success(TaskViewModel.From(task));
    }
}
