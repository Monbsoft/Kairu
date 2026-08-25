namespace KairuFocus.Application.Gamification.Commands.CreditSprintXp;

public sealed record CreditSprintXpResult
{
    public bool IsSuccess { get; init; }
    public int XpAwarded { get; init; }
    public string? Error { get; init; }

    private CreditSprintXpResult() { }

    public static CreditSprintXpResult Success(int xpAwarded) => new() { IsSuccess = true, XpAwarded = xpAwarded };
    public static CreditSprintXpResult Failure(string error) => new() { Error = error };
}
