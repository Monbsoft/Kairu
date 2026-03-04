using Kairudev.Application.Common;
using Kairudev.Domain.Pomodoro;

namespace Kairudev.Application.Pomodoro.Commands.SaveSettings;

public sealed class SaveSettingsCommandHandler
{
    private readonly IPomodoroSettingsRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public SaveSettingsCommandHandler(IPomodoroSettingsRepository repository, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<SaveSettingsResult> HandleAsync(
        SaveSettingsCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;

        var settingsResult = PomodoroSettings.Create(
            command.SprintDurationMinutes,
            command.ShortBreakDurationMinutes,
            command.LongBreakDurationMinutes);

        if (settingsResult.IsFailure)
            return SaveSettingsResult.Validation(settingsResult.Error);

        await _repository.SaveAsync(settingsResult.Value, userId, cancellationToken);
        return SaveSettingsResult.Success();
    }
}
