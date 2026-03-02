using Kairudev.Application.Journal.Common;
using Kairudev.Domain.Journal;
using Kairudev.Domain.Pomodoro;
using Kairudev.Domain.Tasks;

namespace Kairudev.Application.Journal.Queries.GetTodayJournal;

public sealed class GetTodayJournalQueryHandler
{
    private readonly IJournalEntryRepository _repository;
    private readonly IPomodoroSessionRepository _sessionRepository;
    private readonly ITaskRepository _taskRepository;

    public GetTodayJournalQueryHandler(
        IJournalEntryRepository repository,
        IPomodoroSessionRepository sessionRepository,
        ITaskRepository taskRepository)
    {
        _repository = repository;
        _sessionRepository = sessionRepository;
        _taskRepository = taskRepository;
    }

    public async Task<GetTodayJournalResult> HandleAsync(
        GetTodayJournalQuery query,
        CancellationToken cancellationToken = default)
    {
        var entries = await _repository.GetTodayEntriesAsync(DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);
        var viewModels = await JournalEntryMapper.MapToViewModelsAsync(entries, _sessionRepository, _taskRepository, cancellationToken);
        return new GetTodayJournalResult(viewModels);
    }
}
