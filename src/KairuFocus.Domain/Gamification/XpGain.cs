using KairuFocus.Domain.Common;
using KairuFocus.Domain.Identity;

namespace KairuFocus.Domain.Gamification;

/// <summary>
/// Append-only XP ledger entry. Immutable after creation: a gain is a historical
/// fact dated by the session's completion instant (ADR-022).
/// </summary>
public sealed class XpGain : AggregateRoot<XpGainId>
{
    // Parameter names match property names: EF Core materializes via constructor binding
    // (same pattern as PomodoroSession).
    private XpGain(XpGainId id, UserId ownerId, Guid sessionId, int amount, DateTime earnedAtUtc)
        : base(id)
    {
        OwnerId = ownerId;
        SessionId = sessionId;
        Amount = amount;
        EarnedAtUtc = earnedAtUtc;
    }

    public UserId OwnerId { get; private set; } = default!;

    /// <summary>Raw Guid on purpose: no domain FK towards the Pomodoro aggregate (ADR-022).</summary>
    public Guid SessionId { get; private set; }

    public int Amount { get; private set; }
    public DateTime EarnedAtUtc { get; private set; }

    /// <summary>
    /// Records an XP credit. Guards throw: invalid arguments here are programmer
    /// errors (the eligibility decision belongs to <see cref="SprintXpPolicy"/>),
    /// not part of the normal domain flow.
    /// </summary>
    public static XpGain Credit(UserId ownerId, Guid sessionId, int amount, DateTime earnedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(ownerId);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(amount, 0);
        if (earnedAtUtc == default)
            throw new ArgumentException("EarnedAtUtc must be a valid UTC instant.", nameof(earnedAtUtc));

        return new XpGain(XpGainId.New(), ownerId, sessionId, amount, earnedAtUtc);
    }
}
