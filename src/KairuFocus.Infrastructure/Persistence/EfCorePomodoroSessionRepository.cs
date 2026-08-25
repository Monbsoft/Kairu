using KairuFocus.Domain.Identity;
using KairuFocus.Domain.Pomodoro;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace KairuFocus.Infrastructure.Persistence;

internal sealed class EfCorePomodoroSessionRepository : IPomodoroSessionRepository
{
    private readonly KairuFocusDbContext _context;

    public EfCorePomodoroSessionRepository(KairuFocusDbContext context)
    {
        _context = context;
    }

    // SQL Server error numbers for a unique index/constraint violation.
    private const int DuplicateKeyRowError = 2601;
    private const int DuplicateKeyConstraintError = 2627;

    public async Task<bool> TryAddAsync(PomodoroSession session, CancellationToken cancellationToken = default)
    {
        await _context.PomodoroSessions.AddAsync(session, cancellationToken);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex) when (IsUniqueIndexViolation(ex))
        {
            // The "one active session per user" filtered unique index rejected the row:
            // a concurrent request already started a session. Detach the orphan so the
            // scoped DbContext stays usable, and let the caller decide what to do.
            _context.Entry(session).State = EntityState.Detached;
            return false;
        }
    }

    // Restricted to the "one active session per user" index: any other unique constraint
    // added later on this table must not be silently reported as a concurrent start.
    // The index name comes from the EF configuration that declares it, so the two cannot drift.
    private static bool IsUniqueIndexViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException sqlException
        && sqlException.Number is DuplicateKeyRowError or DuplicateKeyConstraintError
        && sqlException.Message.Contains(
            PomodoroSessionConfiguration.ActiveSessionIndexName, StringComparison.Ordinal);

    public async Task<PomodoroSession?> GetByIdAsync(PomodoroSessionId id, CancellationToken cancellationToken = default) =>
        await _context.PomodoroSessions.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<PomodoroSession>> GetByIdsAsync(IEnumerable<PomodoroSessionId> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await _context.PomodoroSessions
            .Where(s => idList.Contains(s.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<PomodoroSession?> GetActiveAsync(UserId userId, CancellationToken cancellationToken = default) =>
        await _context.PomodoroSessions
            .FirstOrDefaultAsync(
                s => s.Status == PomodoroSessionStatus.Active && s.OwnerId == userId,
                cancellationToken);

    public async Task UpdateAsync(PomodoroSession session, CancellationToken cancellationToken = default)
    {
        _context.PomodoroSessions.Update(session);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetCompletedTodayCountAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.PomodoroSessions
            .CountAsync(
                s => s.OwnerId == userId
                     && s.Status == PomodoroSessionStatus.Completed
                     && s.EndedAt.HasValue
                     && s.EndedAt.Value.Date == today,
                cancellationToken);
    }

    public async Task<int> GetCompletedSprintsTodayCountAsync(UserId userId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default) =>
        await _context.PomodoroSessions
            .CountAsync(
                s => s.OwnerId == userId
                     && s.SessionType == PomodoroSessionType.Sprint
                     && s.Status == PomodoroSessionStatus.Completed
                     && s.EndedAt.HasValue
                     && s.EndedAt.Value >= startUtc
                     && s.EndedAt.Value < endUtc,
                cancellationToken);

    public async Task<PomodoroSession?> GetLatestCompletedTodayAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.PomodoroSessions
            .Where(s => s.OwnerId == userId
                        && s.Status == PomodoroSessionStatus.Completed
                        && s.EndedAt.HasValue
                        && s.EndedAt.Value.Date == today)
            .OrderByDescending(s => s.EndedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PomodoroSession>> GetTodaySprintSessionsAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.PomodoroSessions
            .Where(s => s.OwnerId == userId
                        && s.SessionType == PomodoroSessionType.Sprint
                        && s.PlannedDurationMinutes == 0
                        && s.StartedAt.HasValue
                        && s.StartedAt.Value.Date == today
                        && (s.Status == PomodoroSessionStatus.Completed
                            || s.Status == PomodoroSessionStatus.Interrupted))
            .OrderBy(s => s.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PomodoroSession>> GetCompletedSprintSessionsTodayAsync(UserId userId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default) =>
        await _context.PomodoroSessions
            .Where(s => s.OwnerId == userId
                        && s.SessionType == PomodoroSessionType.Sprint
                        && s.Status == PomodoroSessionStatus.Completed
                        && s.EndedAt.HasValue
                        && s.EndedAt.Value >= startUtc
                        && s.EndedAt.Value < endUtc)
            .OrderBy(s => s.StartedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DateTime>> GetCompletedSprintEndTimesAsync(UserId userId, DateTime sinceUtc, CancellationToken cancellationToken = default)
    {
        // Return raw UTC EndedAt values bounded by sinceUtc.
        // Date-to-local bucketing is done in the Application layer (ADR-020).
        // Simple >= comparison is provider-safe (SQL Server and SQLite).
        var endTimes = await _context.PomodoroSessions
            .Where(s => s.OwnerId == userId
                        && s.SessionType == PomodoroSessionType.Sprint
                        && s.Status == PomodoroSessionStatus.Completed
                        && s.EndedAt.HasValue
                        && s.EndedAt.Value >= sinceUtc)
            .Select(s => s.EndedAt!.Value)
            .ToListAsync(cancellationToken);

        return endTimes;
    }

    public async Task<IReadOnlyList<CompletedSprintInterval>> GetCompletedSprintIntervalsAsync(UserId userId, DateTime sinceUtc, CancellationToken cancellationToken = default)
    {
        // Return (StartedAt, EndedAt) pairs bounded by sinceUtc.
        // Local-date bucketing is done in the Application layer (ADR-020/021).
        // Simple >= comparison is provider-safe (SQL Server and SQLite).
        return await _context.PomodoroSessions
            .Where(s => s.OwnerId == userId
                        && s.SessionType == PomodoroSessionType.Sprint
                        && s.Status == PomodoroSessionStatus.Completed
                        && s.StartedAt.HasValue
                        && s.EndedAt.HasValue
                        && s.EndedAt.Value >= sinceUtc)
            .Select(s => new CompletedSprintInterval(s.StartedAt!.Value, s.EndedAt!.Value))
            .ToListAsync(cancellationToken);
    }
}
