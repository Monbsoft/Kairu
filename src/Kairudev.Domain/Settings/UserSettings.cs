using Kairudev.Domain.Common;
using Kairudev.Domain.Identity;

namespace Kairudev.Domain.Settings;

/// <summary>
/// Aggregate root representing user settings.
/// One instance per user, identified by UserId.
/// </summary>
public sealed class UserSettings : AggregateRoot<UserId>
{
    public ThemePreference ThemePreference { get; private set; }
    public RingtonePreference RingtonePreference { get; private set; }
    public string? JiraBaseUrl { get; private set; }
    public string? JiraEmail { get; private set; }
    public string? JiraApiToken { get; private set; }

    // Parameterless constructor required by EF Core for materialization
    private UserSettings() : base(UserId.From(string.Empty)) { }

    private UserSettings(
        UserId userId,
        ThemePreference themePreference,
        RingtonePreference ringtonePreference,
        string? jiraBaseUrl,
        string? jiraEmail,
        string? jiraApiToken) : base(userId)
    {
        ThemePreference = themePreference;
        RingtonePreference = ringtonePreference;
        JiraBaseUrl = jiraBaseUrl;
        JiraEmail = jiraEmail;
        JiraApiToken = jiraApiToken;
    }

    /// <summary>
    /// Creates default user settings for a given user.
    /// </summary>
    public static UserSettings CreateDefault(UserId userId)
    {
        return new UserSettings(userId, ThemePreference.System, RingtonePreference.AlarmClock, null, null, null);
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
        string? jiraBaseUrl,
        string? jiraEmail,
        string? jiraApiToken)
    {
        return new UserSettings(userId, themePreference, ringtonePreference, jiraBaseUrl, jiraEmail, jiraApiToken);
    }
}
