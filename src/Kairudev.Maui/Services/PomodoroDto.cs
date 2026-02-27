namespace Kairudev.Maui.Services;

public sealed record PomodoroSettingsDto(
    int SprintDurationMinutes,
    int ShortBreakDurationMinutes,
    int LongBreakDurationMinutes,
    int SprintsBeforeLongBreak);

public sealed record PomodoroSessionDto(
    Guid Id,
    string SessionType,
    string Status,
    int PlannedDurationMinutes,
    DateTime? StartedAt,
    DateTime? EndedAt,
    IReadOnlyList<Guid> LinkedTaskIds);

public sealed record PomodoroCompleteResultDto(
    string BreakType,
    int BreakDurationMinutes);

public sealed record SuggestedSessionTypeDto(
    string SuggestedType,
    int SprintDurationMinutes,
    int ShortBreakDurationMinutes,
    int LongBreakDurationMinutes);
