namespace Kairu.Application.Settings.Commands.RevokeApiKey;

public sealed record RevokeApiKeyResult
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }

    private RevokeApiKeyResult() { }

    public static RevokeApiKeyResult Success() => new() { IsSuccess = true };
    public static RevokeApiKeyResult Failure(string error) => new() { IsSuccess = false, Error = error };
}
