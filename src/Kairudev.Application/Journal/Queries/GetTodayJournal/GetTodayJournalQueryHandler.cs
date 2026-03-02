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
        var entries = await _repository.GetEntriesByDateAsync(DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);

        var allTasks = await _taskRepository.GetAllAsync(cancellationToken);
        var taskLookup = allTasks.ToDictionary(t => t.Id.Value, t => t.Title.Value);

        var sessionIds = entries
            .Where(e => PomodoroEventTypes.Contains(e.EventType))
            .Select(e => PomodoroSessionId.From(e.ResourceId))
            .Distinct()
            .ToList();

        var sessions = await _sessionRepository.GetByIdsAsync(sessionIds, cancellationToken);
        var sessionLookup = sessions.ToDictionary(s => s.Id);

        var viewModels = new List<JournalEntryViewModel>(entries.Count);
        foreach (var entry in entries)
        {
            IReadOnlyList<string>? taskTitles = null;
            if (PomodoroEventTypes.Contains(entry.EventType))
            {
                var sessionId = PomodoroSessionId.From(entry.ResourceId);
                if (sessionLookup.TryGetValue(sessionId, out var session) && session.LinkedTaskIds.Count > 0)
                {
                    taskTitles = session.LinkedTaskIds
                        .Where(id => taskLookup.ContainsKey(id.Value))
                        .Select(id => taskLookup[id.Value])
                        .ToList()
                        .AsReadOnly();
                }
            }
            viewModels.Add(JournalEntryViewModel.From(entry, taskTitles));
        }

        return new GetTodayJournalResult(viewModels);
    }
}
