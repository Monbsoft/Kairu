using Kairudev.Application.Common;
using Kairudev.Application.Journal.Common;
using Kairudev.Domain.Journal;
using Kairudev.Domain.Pomodoro;
using Kairudev.Domain.Tasks;

namespace Kairudev.Application.Journal.Queries.GetJournalByDate;

public sealed class GetJournalByDateQueryHandler
{
    private readonly IJournalEntryRepository _repository;
    private readonly IPomodoroSessionRepository _sessionRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetJournalByDateQueryHandler(
        IJournalEntryRepository repository,
        IPomodoroSessionRepository sessionRepository,
        ITaskRepository taskRepository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _sessionRepository = sessionRepository;
        _taskRepository = taskRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetJournalByDateResult> HandleAsync(
        GetJournalByDateQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;
        var entries = await _repository.GetEntriesByDateAsync(query.Date, userId, cancellationToken);
        var viewModels = await JournalEntryMapper.MapToViewModelsAsync(
            entries, _sessionRepository, _taskRepository, userId, cancellationToken);

        return new GetJournalByDateResult(viewModels);
    }
}
