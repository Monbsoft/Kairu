namespace Kairudev.Application.Settings.Common;

public sealed record UserSettingsViewModel(
    string ThemePreference,
    string RingtonePreference,
    string? JiraBaseUrl,
    string? JiraEmail,
    bool JiraConfigured);
