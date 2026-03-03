namespace Kairudev.Application.Settings.Commands.SaveJiraSettings;

public sealed record SaveJiraSettingsCommand(string? JiraBaseUrl, string? JiraEmail, string? JiraApiToken);
