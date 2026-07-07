using KairuFocus.Application.Common;
using KairuFocus.Domain.Settings;
using Microsoft.Extensions.Logging;
using Monbsoft.BrilliantMediator.Abstractions.Commands;

namespace KairuFocus.Application.Settings.Commands.SaveAlarmVolume;

public sealed class SaveAlarmVolumeCommandHandler : ICommandHandler<SaveAlarmVolumeCommand, SaveAlarmVolumeResult>
{
    private readonly IUserSettingsRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SaveAlarmVolumeCommandHandler> _logger;

    public SaveAlarmVolumeCommandHandler(
        IUserSettingsRepository repository,
        ICurrentUserService currentUserService,
        ILogger<SaveAlarmVolumeCommandHandler> logger)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<SaveAlarmVolumeResult> Handle(SaveAlarmVolumeCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Volume is < 0 or > 100)
        {
            return SaveAlarmVolumeResult.Failure($"Invalid alarm volume '{command.Volume}'. Must be between 0 and 100.");
        }

        var userId = _currentUserService.CurrentUserId;
        _logger.LogDebug("Saving alarm volume {Volume} for user {UserId}", command.Volume, userId);
        var settings = await _repository.GetByUserIdAsync(userId);
        settings.UpdateAlarmVolume(command.Volume);
        await _repository.UpdateAsync(settings);

        _logger.LogInformation("Alarm volume updated to {Volume} for user {UserId}", command.Volume, userId);
        return SaveAlarmVolumeResult.Success();
    }
}
