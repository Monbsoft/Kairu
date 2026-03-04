namespace Kairudev.Infrastructure.Persistence.Internal;

/// <summary>EF Core row for PomodoroSettings — one row per user identified by UserId string.</summary>
internal sealed class PomodoroSettingsRow
{
    public string UserId { get; set; } = string.Empty;
    public int SprintDurationMinutes { get; set; }
    public int ShortBreakDurationMinutes { get; set; }
    public int LongBreakDurationMinutes { get; set; }
}
