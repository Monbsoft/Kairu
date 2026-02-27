using Kairudev.Domain.Pomodoro;
using Microsoft.EntityFrameworkCore;

namespace Kairudev.Infrastructure.Persistence;

internal sealed class SqlitePomodoroSessionRepository : IPomodoroSessionRepository
{
    private readonly KairudevDbContext _context;

    public SqlitePomodoroSessionRepository(KairudevDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PomodoroSession session, CancellationToken cancellationToken = default)
    {
        await _context.PomodoroSessions.AddAsync(session, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PomodoroSession?> GetByIdAsync(PomodoroSessionId id, CancellationToken cancellationToken = default) =>
        await _context.PomodoroSessions.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<PomodoroSession>> GetByIdsAsync(IEnumerable<PomodoroSessionId> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await _context.PomodoroSessions
            .Where(s => idList.Contains(s.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<PomodoroSession?> GetActiveAsync(CancellationToken cancellationToken = default) =>
        await _context.PomodoroSessions
            .FirstOrDefaultAsync(s => s.Status == PomodoroSessionStatus.Active, cancellationToken);

    public async Task UpdateAsync(PomodoroSession session, CancellationToken cancellationToken = default)
    {
        _context.PomodoroSessions.Update(session);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetCompletedTodayCountAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.PomodoroSessions
            .CountAsync(
                s => s.Status == PomodoroSessionStatus.Completed
                     && s.EndedAt.HasValue
                     && s.EndedAt.Value.Date == today,
                cancellationToken);
    }

    public async Task<int> GetCompletedSprintsTodayCountAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.PomodoroSessions
            .CountAsync(
                s => s.SessionType == PomodoroSessionType.Sprint
                     && s.Status == PomodoroSessionStatus.Completed
                     && s.EndedAt.HasValue
                     && s.EndedAt.Value.Date == today,
                cancellationToken);
    }

    public async Task<PomodoroSession?> GetLatestCompletedTodayAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.PomodoroSessions
            .Where(s => s.Status == PomodoroSessionStatus.Completed
                        && s.EndedAt.HasValue
                        && s.EndedAt.Value.Date == today)
            .OrderByDescending(s => s.EndedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
