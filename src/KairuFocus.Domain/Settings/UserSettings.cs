using KairuFocus.Domain.Common;
using KairuFocus.Domain.Identity;

namespace KairuFocus.Domain.Settings;

/// <summary>
/// Aggregate root representing user settings.
/// One instance per user, identified by UserId.
/// </summary>
public sealed class UserSettings : AggregateRoot<UserId>
{
    public ThemePreference ThemePreference { get; private set; }
    public RingtonePreference RingtonePreference { get; private set; }
    public int AlarmVolume { get; private set; }
    public string? JiraBaseUrl { get; private set; }
    public string? JiraEmail { get; private set; }
    public string? JiraApiToken { get; private set; }

    // Parameterless constructor required by EF Core for materialization
    private UserSettings() : base(null!) { }

    private UserSettings(
        UserId userId,
        ThemePreference themePreference,
        RingtonePreference ringtonePreference,
        int alarmVolume,
        string? jiraBaseUrl,
        string? jiraEmail,
        string? jiraApiToken) : base(userId)
    {
        ThemePreference = themePreference;
        RingtonePreference = ringtonePreference;
        AlarmVolume = alarmVolume;
        JiraBaseUrl = jiraBaseUrl;
        JiraEmail = jiraEmail;
        JiraApiToken = jiraApiToken;
    }

    /// <summary>
    /// Creates default user settings for a given user.
    /// </summary>
    public static UserSettings CreateDefault(UserId userId)
    {
        return new UserSettings(userId, ThemePreference.System, RingtonePreference.AlarmClock, 80, null, null, null);
    }

    /// <summary>
    /// Updates the theme preference.
    /// </summary>
    public void UpdateThemePreference(ThemePreference newPreference)
    {
        ThemePreference = newPreference;
    }

    /// <summary>
    /// Updates the ringtone preference.
    /// </summary>
    public void UpdateRingtonePreference(RingtonePreference newPreference)
    {
        RingtonePreference = newPreference;
    }

    /// <summary>
    /// Updates the alarm volume (percentage, clamped to 0–100).
    /// </summary>
    public void UpdateAlarmVolume(int newVolume)
    {
        AlarmVolume = Math.Clamp(newVolume, 0, 100);
    }

    /// <summary>
    /// Updates the Jira integration settings.
    /// </summary>
    public void UpdateJiraSettings(string? baseUrl, string? email, string? apiToken)
    {
        JiraBaseUrl = baseUrl;
        JiraEmail = email;
        JiraApiToken = apiToken;
    }

    /// <summary>
    /// Reconstitutes a UserSettings from persistence.
    /// </summary>
    public static UserSettings Reconstitute(
        UserId userId,
        ThemePreference themePreference,
        RingtonePreference ringtonePreference,
        int alarmVolume,
        string? jiraBaseUrl,
        string? jiraEmail,
        string? jiraApiToken)
    {
        return new UserSettings(userId, themePreference, ringtonePreference, alarmVolume, jiraBaseUrl, jiraEmail, jiraApiToken);
    }
}
