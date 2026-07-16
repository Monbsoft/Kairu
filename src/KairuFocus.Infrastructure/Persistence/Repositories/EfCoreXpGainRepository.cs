using KairuFocus.Domain.Gamification;
using KairuFocus.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace KairuFocus.Infrastructure.Persistence.Repositories;

internal sealed class EfCoreXpGainRepository : IXpGainRepository
{
    private readonly KairuFocusDbContext _context;

    public EfCoreXpGainRepository(KairuFocusDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(XpGain gain, CancellationToken cancellationToken = default)
    {
        await _context.XpGains.AddAsync(gain, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsForSessionAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
        await _context.XpGains.AnyAsync(g => g.SessionId == sessionId, cancellationToken);

    public async Task<int> GetTotalXpAsync(UserId userId, CancellationToken cancellationToken = default) =>
        // Nullable projection: SUM over an empty set yields NULL in SQL — coalesce to 0.
        await _context.XpGains
            .Where(g => g.OwnerId == userId)
            .Select(g => (int?)g.Amount)
            .SumAsync(cancellationToken) ?? 0;

    public async Task<int> GetXpBetweenAsync(UserId userId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default) =>
        // UTC range comparison [startUtc, endUtc) — provider-safe (ADR-020/021).
        await _context.XpGains
            .Where(g => g.OwnerId == userId
                        && g.EarnedAtUtc >= startUtc
                        && g.EarnedAtUtc < endUtc)
            .Select(g => (int?)g.Amount)
            .SumAsync(cancellationToken) ?? 0;
}
