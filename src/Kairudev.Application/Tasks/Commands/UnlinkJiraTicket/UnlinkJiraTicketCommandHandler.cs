using Kairudev.Domain.Tasks;

namespace Kairudev.Application.Tasks.Commands.UnlinkJiraTicket;

public sealed class UnlinkJiraTicketCommandHandler
{
    private readonly ITaskRepository _repository;

    public UnlinkJiraTicketCommandHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<UnlinkJiraTicketResult> HandleAsync(
        UnlinkJiraTicketCommand command,
        CancellationToken cancellationToken = default)
    {
        var task = await _repository.GetByIdAsync(TaskId.From(command.TaskId), cancellationToken);
        if (task is null)
            return UnlinkJiraTicketResult.NotFound();

        task.UnlinkJiraTicket();
        await _repository.UpdateAsync(task, cancellationToken);

        return UnlinkJiraTicketResult.Success();
    }
}
