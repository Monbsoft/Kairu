using Kairudev.Domain.Identity;

namespace Kairudev.Domain.Journal;

public interface IJournalEntryRepository
{
    Task AddAsync(JournalEntry entry, CancellationToken cancellationToken = default);
    Task<JournalEntry?> GetByIdAsync(JournalEntryId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JournalEntry>> GetEntriesByDateAsync(DateOnly date, UserId userId, CancellationToken cancellationToken = default);
    Task<int> GetTodayCountByTypeAsync(JournalEventType eventType, DateOnly today, UserId userId, CancellationToken cancellationToken = default);
    Task UpdateAsync(JournalEntry entry, CancellationToken cancellationToken = default);
}
