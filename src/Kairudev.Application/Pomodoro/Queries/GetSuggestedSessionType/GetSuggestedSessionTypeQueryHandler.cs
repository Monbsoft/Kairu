using Kairudev.Domain.Pomodoro;

namespace Kairudev.Application.Pomodoro.Queries.GetSuggestedSessionType;

public sealed class GetSuggestedSessionTypeQueryHandler
{
    private readonly IPomodoroSessionRepository _sessionRepository;
    private readonly IPomodoroSettingsRepository _settingsRepository;

    public GetSuggestedSessionTypeQueryHandler(
        IPomodoroSessionRepository sessionRepository,
        IPomodoroSettingsRepository settingsRepository)
    {
        _sessionRepository = sessionRepository;
        _settingsRepository = settingsRepository;
    }

    public async Task<GetSuggestedSessionTypeResult> HandleAsync(
        GetSuggestedSessionTypeQuery query,
        CancellationToken cancellationToken = default)
    {
        var settings = await _settingsRepository.GetAsync(cancellationToken);
        var latestSession = await _sessionRepository.GetLatestCompletedTodayAsync(cancellationToken);

        PomodoroSessionType suggestedType;
        if (latestSession?.SessionType == PomodoroSessionType.Sprint)
        {
            var sprintsToday = await _sessionRepository.GetCompletedSprintsTodayCountAsync(cancellationToken);
            // Dernier cycle était un sprint → suggérer une pause
            suggestedType = sprintsToday % PomodoroSettings.SprintsBeforeLongBreak == 0
                ? PomodoroSessionType.LongBreak
                : PomodoroSessionType.ShortBreak;
        }
        else
        {
            // Dernier cycle était une pause (ou aucune session) → suggérer un sprint
            suggestedType = PomodoroSessionType.Sprint;
        }

        return new GetSuggestedSessionTypeResult(
            suggestedType.ToString(),
            settings.SprintDurationMinutes,
            settings.ShortBreakDurationMinutes,
            settings.LongBreakDurationMinutes);
    }
}
