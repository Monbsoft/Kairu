namespace KairuFocus.Application.Pomodoro.Commands.UnlinkTask;

public sealed record UnlinkTaskResult
{
    public bool IsSuccess { get; init; }
    public bool IsNotFound { get; init; }
    public string? Error { get; init; }

    private UnlinkTaskResult() { }

    public static UnlinkTaskResult Success() => new() { IsSuccess = true };
    public static UnlinkTaskResult NotFound() => new() { IsNotFound = true };
    public static UnlinkTaskResult Failure(string error) => new() { Error = error };
}
