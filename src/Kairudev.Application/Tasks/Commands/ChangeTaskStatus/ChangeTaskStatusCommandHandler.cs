using Kairudev.Application.Common;
using Kairudev.Application.Journal.Commands.CreateEntry;
using Kairudev.Application.Tasks.Common;
using Kairudev.Domain.Journal;
using Kairudev.Domain.Tasks;
using TaskStatus = Kairudev.Domain.Tasks.TaskStatus;

namespace Kairudev.Application.Tasks.Commands.ChangeTaskStatus;

public sealed class ChangeTaskStatusCommandHandler
{
    private readonly ITaskRepository _repository;
    private readonly CreateEntryCommandHandler _journalHandler;
    private readonly ICurrentUserService _currentUserService;

    public ChangeTaskStatusCommandHandler(
        ITaskRepository repository,
        CreateEntryCommandHandler journalHandler,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _journalHandler = journalHandler;
        _currentUserService = currentUserService;
    }

    public async Task<ChangeTaskStatusResult> HandleAsync(
        ChangeTaskStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;

        var task = await _repository.GetByIdAsync(TaskId.From(command.TaskId), userId, cancellationToken);
        if (task is null)
            return ChangeTaskStatusResult.NotFound();

        // Parse status string to enum
        if (!Enum.TryParse<TaskStatus>(command.NewStatus, true, out var newStatus))
            return ChangeTaskStatusResult.Validation($"Invalid status: {command.NewStatus}");

        var changeResult = task.ChangeStatus(newStatus, DateTime.UtcNow);
        if (changeResult.IsFailure)
            return ChangeTaskStatusResult.Conflict(changeResult.Error);

        await _repository.UpdateAsync(task, cancellationToken);

        // Generate journal entries based on status
        if (newStatus == TaskStatus.InProgress)
        {
            await _journalHandler.HandleAsync(
                new CreateEntryCommand(
                    JournalEventType.TaskStarted,
                    task.Id.Value,
                    DateTime.UtcNow,
                    userId),
                cancellationToken);
        }
        else if (newStatus == TaskStatus.Done)
        {
            await _journalHandler.HandleAsync(
                new CreateEntryCommand(
                    JournalEventType.TaskCompleted,
                    task.Id.Value,
                    DateTime.UtcNow,
                    userId),
                cancellationToken);
        }

        return ChangeTaskStatusResult.Success(TaskViewModel.From(task));
    }
}
