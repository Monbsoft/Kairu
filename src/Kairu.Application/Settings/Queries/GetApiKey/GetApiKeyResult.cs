namespace Kairu.Application.Settings.Queries.GetApiKey;

public sealed record GetApiKeyResult(bool Exists, DateTime? CreatedAt);
