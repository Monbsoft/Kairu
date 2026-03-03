namespace Kairudev.Application.Settings.Commands.SaveJiraSettings;

public sealed record SaveJiraSettingsResult
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }

    private SaveJiraSettingsResult() { }

    public static SaveJiraSettingsResult Success() => new() { IsSuccess = true };
    public static SaveJiraSettingsResult Failure(string error) => new() { IsSuccess = false, Error = error };
}
