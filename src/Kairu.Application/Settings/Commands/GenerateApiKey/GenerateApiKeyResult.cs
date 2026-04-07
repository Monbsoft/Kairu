namespace Kairu.Application.Settings.Commands.GenerateApiKey;

public sealed record GenerateApiKeyResult
{
    public bool IsSuccess { get; init; }
    /// <summary>Token brut — retourné une seule fois, jamais stocké en clair.</summary>
    public string? Token { get; init; }
    public string? Error { get; init; }

    private GenerateApiKeyResult() { }

    public static GenerateApiKeyResult Success(string token) =>
        new() { IsSuccess = true, Token = token };

    public static GenerateApiKeyResult Failure(string error) =>
        new() { IsSuccess = false, Error = error };
}
