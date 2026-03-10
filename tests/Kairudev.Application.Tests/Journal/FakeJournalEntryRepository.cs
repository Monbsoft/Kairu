using Kairudev.Domain.Identity;
using Kairudev.Domain.Journal;

namespace Kairudev.Application.Tests.Journal;

internal sealed class FakeJournalEntryRepository : IJournalEntryRepository
{
    public List<JournalEntry> Entries { get; } = [];

    public Task AddAsync(JournalEntry entry, CancellationToken cancellationToken = default)
    {
        Entries.Add(entry);
        return Task.CompletedTask;
    }

    public Task<JournalEntry?> GetByIdAsync(JournalEntryId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Entries.FirstOrDefault(e => e.Id == id));

    public Task<IReadOnlyList<JournalEntry>> GetEntriesByDateAsync(DateOnly date, UserId userId, CancellationToken cancellationToken = default)
    {
        var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end   = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
        IReadOnlyList<JournalEntry> result = Entries
            .Where(e => e.OwnerId == userId && e.OccurredAt >= start && e.OccurredAt <= end)
            .OrderBy(e => e.OccurredAt)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<int> GetTodayCountByTypeAsync(JournalEventType eventType, DateOnly today, UserId userId, CancellationToken cancellationToken = default)
    {
        var start = today.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end   = today.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
        return Task.FromResult(Entries.Count(e => e.OwnerId == userId && e.EventType == eventType && e.OccurredAt >= start && e.OccurredAt <= end));
    }

    public Task UpdateAsync(JournalEntry entry, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
