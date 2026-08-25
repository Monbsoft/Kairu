using KairuFocus.Domain.Identity;

namespace KairuFocus.Domain.Gamification;

public interface IXpGainRepository
{
    Task AddAsync(XpGain gain, CancellationToken cancellationToken = default);

    /// <summary>Idempotence pre-check: has this session already been credited?</summary>
    Task<bool> ExistsForSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);

    Task<int> GetTotalXpAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sum of XP earned within [startUtc, endUtc).
    /// Uses UTC range comparison (provider-safe: no date bucketing in SQL, ADR-020/021).
    /// </summary>
    Task<int> GetXpBetweenAsync(UserId userId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default);
}
