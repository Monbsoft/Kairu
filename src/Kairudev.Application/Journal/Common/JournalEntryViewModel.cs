using Kairudev.Domain.Journal;

namespace Kairudev.Application.Journal.Common;

public sealed record JournalCommentViewModel(Guid Id, string Text);

public sealed record JournalEntryViewModel(
    Guid Id,
    DateTime OccurredAt,
    string EventType,
    Guid ResourceId,
    int? Sequence,
    IReadOnlyList<string> LinkedTaskTitles,
    IReadOnlyList<JournalCommentViewModel> Comments)
{
    public static JournalEntryViewModel From(JournalEntry entry, IReadOnlyList<string>? taskTitles = null) => new(
        entry.Id.Value,
        entry.OccurredAt,
        entry.EventType.ToString(),
        entry.ResourceId,
        entry.Sequence,
        taskTitles ?? [],
        entry.Comments.Select(c => new JournalCommentViewModel(c.Id.Value, c.Text)).ToList().AsReadOnly());
}
