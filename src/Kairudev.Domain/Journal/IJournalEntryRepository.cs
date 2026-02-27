namespace Kairudev.Domain.Journal;

public interface IJournalEntryRepository
{
    Task AddAsync(JournalEntry entry, CancellationToken cancellationToken = default);
    Task<JournalEntry?> GetByIdAsync(JournalEntryId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JournalEntry>> GetTodayEntriesAsync(DateOnly today, CancellationToken cancellationToken = default);
    Task<int> GetTodayCountByTypeAsync(JournalEventType eventType, DateOnly today, CancellationToken cancellationToken = default);
    Task UpdateAsync(JournalEntry entry, CancellationToken cancellationToken = default);
}
