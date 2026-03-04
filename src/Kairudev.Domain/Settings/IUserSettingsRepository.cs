using Kairudev.Domain.Identity;

namespace Kairudev.Domain.Settings;

/// <summary>
/// Repository for user settings.
/// </summary>
public interface IUserSettingsRepository
{
    /// <summary>
    /// Gets the user settings for the given user.
    /// Creates default settings if none exist.
    /// </summary>
    Task<UserSettings> GetByUserIdAsync(UserId userId);

    /// <summary>
    /// Updates the user settings.
    /// </summary>
    Task UpdateAsync(UserSettings settings);
}
