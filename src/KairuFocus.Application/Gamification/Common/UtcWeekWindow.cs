namespace KairuFocus.Application.Gamification.Common;

/// <summary>
/// UTC calendar week window: Monday 00:00 UTC (inclusive) to next Monday 00:00 UTC (exclusive).
/// Uniform for weekly XP and the leaderboard (ADR-022): global fairness over timezone comfort.
/// </summary>
public sealed record UtcWeekWindow(DateTime StartUtc, DateTime EndUtc)
{
    public static UtcWeekWindow From(DateTime utcNow)
    {
        var todayUtc = utcNow.Date;
        // DayOfWeek: Sunday = 0 … Saturday = 6 → shift so that Monday = 0 … Sunday = 6.
        var daysSinceMonday = ((int)todayUtc.DayOfWeek + 6) % 7;
        var startUtc = todayUtc.AddDays(-daysSinceMonday);
        return new UtcWeekWindow(startUtc, startUtc.AddDays(7));
    }
}
