using Kairudev.Domain.Identity;
using Kairudev.Domain.Journal;
using Microsoft.EntityFrameworkCore;

namespace Kairudev.Infrastructure.Persistence.Repositories;

public sealed class SqliteJournalEntryRepository : IJournalEntryRepository
{
    private readonly KairudevDbContext _context;

    public SqliteJournalEntryRepository(KairudevDbContext context) => _context = context;

    public async Task AddAsync(JournalEntry entry, CancellationToken cancellationToken = default)
    {
        await _context.JournalEntries.AddAsync(entry, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<JournalEntry?> GetByIdAsync(JournalEntryId id, CancellationToken cancellationToken = default)
        => await _context.JournalEntries
            .Include(e => e.Comments)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<JournalEntry>> GetEntriesByDateAsync(DateOnly date, UserId userId, CancellationToken cancellationToken = default)
    {
        var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end   = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
        return await _context.JournalEntries
            .Include(e => e.Comments)
            .Where(e => e.OwnerId == userId && e.OccurredAt >= start && e.OccurredAt <= end)
            .OrderBy(e => e.OccurredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTodayCountByTypeAsync(JournalEventType eventType, DateOnly today, UserId userId, CancellationToken cancellationToken = default)
    {
        var start = today.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end   = today.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
        return await _context.JournalEntries
            .CountAsync(
                e => e.OwnerId == userId && e.EventType == eventType && e.OccurredAt >= start && e.OccurredAt <= end,
                cancellationToken);
    }

    public async Task UpdateAsync(JournalEntry entry, CancellationToken cancellationToken = default)
    {
        _context.JournalEntries.Update(entry);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
