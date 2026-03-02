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

    public GetJournalByDateQueryHandler(
        IJournalEntryRepository repository,
        IPomodoroSessionRepository sessionRepository,
        ITaskRepository taskRepository)
    {
        _repository = repository;
        _sessionRepository = sessionRepository;
        _taskRepository = taskRepository;
    }

    public async Task<GetJournalByDateResult> HandleAsync(
        GetJournalByDateQuery query,
        CancellationToken cancellationToken = default)
    {
        var entries = await _repository.GetTodayEntriesAsync(query.Date, cancellationToken);
        var viewModels = await JournalEntryMapper.MapToViewModelsAsync(entries, _sessionRepository, _taskRepository, cancellationToken);
        return new GetJournalByDateResult(viewModels);
    }
}
