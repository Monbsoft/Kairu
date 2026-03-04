using Kairudev.Application.Common;
using Kairudev.Domain.Tasks;

namespace Kairudev.Application.Tasks.Commands.LinkJiraTicket;

public sealed class LinkJiraTicketCommandHandler
{
    private readonly ITaskRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public LinkJiraTicketCommandHandler(ITaskRepository repository, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<LinkJiraTicketResult> HandleAsync(
        LinkJiraTicketCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;

        var task = await _repository.GetByIdAsync(TaskId.From(command.TaskId), userId, cancellationToken);
        if (task is null)
            return LinkJiraTicketResult.NotFound();

        var keyResult = JiraTicketKey.Create(command.JiraTicketKey);
        if (keyResult.IsFailure)
            return LinkJiraTicketResult.Failure(keyResult.Error);

        task.LinkJiraTicket(keyResult.Value);
        await _repository.UpdateAsync(task, cancellationToken);

        return LinkJiraTicketResult.Success();
    }
}
