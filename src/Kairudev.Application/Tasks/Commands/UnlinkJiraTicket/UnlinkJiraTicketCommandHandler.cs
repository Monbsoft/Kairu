using Kairudev.Application.Common;
using Kairudev.Domain.Tasks;

namespace Kairudev.Application.Tasks.Commands.UnlinkJiraTicket;

public sealed class UnlinkJiraTicketCommandHandler
{
    private readonly ITaskRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public UnlinkJiraTicketCommandHandler(ITaskRepository repository, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<UnlinkJiraTicketResult> HandleAsync(
        UnlinkJiraTicketCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;

        var task = await _repository.GetByIdAsync(TaskId.From(command.TaskId), userId, cancellationToken);
        if (task is null)
            return UnlinkJiraTicketResult.NotFound();

        task.UnlinkJiraTicket();
        await _repository.UpdateAsync(task, cancellationToken);

        return UnlinkJiraTicketResult.Success();
    }
}
