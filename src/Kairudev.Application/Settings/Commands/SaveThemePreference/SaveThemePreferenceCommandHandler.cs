using Kairudev.Application.Common;
using Kairudev.Domain.Common;
using Kairudev.Domain.Settings;

namespace Kairudev.Application.Settings.Commands.SaveThemePreference;

public sealed class SaveThemePreferenceCommandHandler
{
    private readonly IUserSettingsRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public SaveThemePreferenceCommandHandler(IUserSettingsRepository repository, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<SaveThemePreferenceResult> Handle(SaveThemePreferenceCommand command)
    {
        // Parse theme preference
        if (!Enum.TryParse<ThemePreference>(command.ThemePreference, ignoreCase: true, out var themePreference))
        {
            return SaveThemePreferenceResult.Failure($"Invalid theme preference '{command.ThemePreference}'. Valid values: Light, Dark, System.");
        }

        var userId = _currentUserService.CurrentUserId;

        // Get settings (creates default if not exists)
        var settings = await _repository.GetByUserIdAsync(userId);

        // Update theme preference
        settings.UpdateThemePreference(themePreference);

        // Persist
        await _repository.UpdateAsync(settings);

        return SaveThemePreferenceResult.Success();
    }
}
