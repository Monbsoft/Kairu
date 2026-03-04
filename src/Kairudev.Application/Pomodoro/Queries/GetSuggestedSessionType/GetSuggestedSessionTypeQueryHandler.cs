using Kairudev.Application.Common;
using Kairudev.Domain.Pomodoro;

namespace Kairudev.Application.Pomodoro.Queries.GetSuggestedSessionType;

public sealed class GetSuggestedSessionTypeQueryHandler
{
    private readonly IPomodoroSessionRepository _sessionRepository;
    private readonly IPomodoroSettingsRepository _settingsRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetSuggestedSessionTypeQueryHandler(
        IPomodoroSessionRepository sessionRepository,
        IPomodoroSettingsRepository settingsRepository,
        ICurrentUserService currentUserService)
    {
        _sessionRepository = sessionRepository;
        _settingsRepository = settingsRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetSuggestedSessionTypeResult> HandleAsync(
        GetSuggestedSessionTypeQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;

        var settings = await _settingsRepository.GetByUserIdAsync(userId, cancellationToken);
        var latestSession = await _sessionRepository.GetLatestCompletedTodayAsync(userId, cancellationToken);

        PomodoroSessionType suggestedType;
        if (latestSession?.SessionType == PomodoroSessionType.Sprint)
        {
            var sprintsToday = await _sessionRepository.GetCompletedSprintsTodayCountAsync(userId, cancellationToken);
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
            suggestedType,
            settings.SprintDurationMinutes,
            settings.ShortBreakDurationMinutes,
            settings.LongBreakDurationMinutes);
    }
}
