namespace KairuFocus.Application.Settings.Common;

public sealed record UserSettingsViewModel(
    string ThemePreference,
    string RingtonePreference,
    int AlarmVolume,
    string? JiraBaseUrl,
    string? JiraEmail,
    bool JiraConfigured);
