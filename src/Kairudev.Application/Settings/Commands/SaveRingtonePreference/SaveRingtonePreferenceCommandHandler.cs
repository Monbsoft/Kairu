using Kairudev.Application.Common;
using Kairudev.Domain.Settings;

namespace Kairudev.Application.Settings.Commands.SaveRingtonePreference;

public sealed class SaveRingtonePreferenceCommandHandler
{
    private readonly IUserSettingsRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public SaveRingtonePreferenceCommandHandler(IUserSettingsRepository repository, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<SaveRingtonePreferenceResult> Handle(SaveRingtonePreferenceCommand command)
    {
        if (!Enum.TryParse<RingtonePreference>(command.RingtonePreference, ignoreCase: true, out var ringtonePreference))
        {
            return SaveRingtonePreferenceResult.Failure($"Invalid ringtone preference '{command.RingtonePreference}'. Valid values: None, AlarmClock, Bird.");
        }

        var userId = _currentUserService.CurrentUserId;
        var settings = await _repository.GetByUserIdAsync(userId);
        settings.UpdateRingtonePreference(ringtonePreference);
        await _repository.UpdateAsync(settings);

        return SaveRingtonePreferenceResult.Success();
    }
}
