using KairuFocus.Domain.Pomodoro;

namespace KairuFocus.Domain.Gamification;

/// <summary>
/// Eligibility rule for sprint XP, centralized and testable without I/O.
/// Reads Pomodoro facts (enums) — one-way dependency Gamification → Pomodoro,
/// same assembly, accepted per ADR-023.
/// </summary>
public static class SprintXpPolicy
{
    public const int XpPerCompletedSprint = 10;
    public const int MinimumEligibleDurationMinutes = 5;

    /// <summary>
    /// XP amount for a finished session. 0 when not eligible
    /// (break, interrupted, missing dates, or effective duration under 5 minutes).
    /// Uniform for standard and free sprints.
    /// </summary>
    public static int Evaluate(
        PomodoroSessionType sessionType,
        PomodoroSessionStatus status,
        DateTime? startedAt,
        DateTime? endedAt)
    {
        if (sessionType != PomodoroSessionType.Sprint)
            return 0;

        if (status != PomodoroSessionStatus.Completed)
            return 0;

        if (!startedAt.HasValue || !endedAt.HasValue)
            return 0;

        var effectiveDuration = endedAt.Value - startedAt.Value;
        if (effectiveDuration < TimeSpan.FromMinutes(MinimumEligibleDurationMinutes))
            return 0;

        return XpPerCompletedSprint;
    }
}
