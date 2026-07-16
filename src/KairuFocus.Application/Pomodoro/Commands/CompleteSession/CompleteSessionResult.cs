namespace KairuFocus.Application.Pomodoro.Commands.CompleteSession;

public sealed record CompleteSessionResult
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }

    /// <summary>XP credited for this completion (0 when not eligible or when the credit failed).</summary>
    public int XpAwarded { get; init; }

    private CompleteSessionResult() { }

    public static CompleteSessionResult Success(int xpAwarded = 0) => new() { IsSuccess = true, XpAwarded = xpAwarded };
    public static CompleteSessionResult Failure(string error) => new() { Error = error };
}
