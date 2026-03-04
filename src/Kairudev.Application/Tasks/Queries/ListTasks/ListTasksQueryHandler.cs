using Kairudev.Application.Common;
using Kairudev.Application.Tasks.Common;
using Kairudev.Domain.Tasks;

namespace Kairudev.Application.Tasks.Queries.ListTasks;

/// <summary>
/// Handles the ListTasks query.
/// </summary>
public sealed class ListTasksQueryHandler
{
    private readonly ITaskRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public ListTasksQueryHandler(ITaskRepository repository, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<ListTasksResult> HandleAsync(
        ListTasksQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;
        var tasks = await _repository.GetAllAsync(userId, cancellationToken);
        var viewModels = tasks.Select(TaskViewModel.From).ToList();
        return new ListTasksResult(viewModels);
    }
}
