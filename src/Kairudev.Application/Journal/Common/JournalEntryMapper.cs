using Kairudev.Domain.Journal;
using Kairudev.Domain.Pomodoro;
using Kairudev.Domain.Tasks;

namespace Kairudev.Application.Journal.Common;

internal static class JournalEntryMapper
{
    internal static readonly HashSet<JournalEventType> PomodoroEventTypes =
    [
        JournalEventType.SprintStarted,
        JournalEventType.SprintCompleted,
        JournalEventType.SprintInterrupted,
        JournalEventType.BreakStarted,
        JournalEventType.BreakCompleted,
        JournalEventType.BreakInterrupted,
    ];

    internal static async Task<List<JournalEntryViewModel>> MapToViewModelsAsync(
        IReadOnlyList<JournalEntry> entries,
        IPomodoroSessionRepository sessionRepository,
        ITaskRepository taskRepository,
        CancellationToken cancellationToken)
    {
        var allTasks = await taskRepository.GetAllAsync(cancellationToken);
        var taskLookup = allTasks.ToDictionary(t => t.Id.Value, t => t.Title.Value);

        var sessionIds = entries
            .Where(e => PomodoroEventTypes.Contains(e.EventType))
            .Select(e => PomodoroSessionId.From(e.ResourceId))
            .Distinct()
            .ToList();

        var sessions = await sessionRepository.GetByIdsAsync(sessionIds, cancellationToken);
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

        return viewModels;
    }
}
