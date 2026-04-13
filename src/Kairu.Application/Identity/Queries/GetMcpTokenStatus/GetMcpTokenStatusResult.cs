namespace Kairu.Application.Identity.Queries.GetMcpTokenStatus;

/// <summary>
/// Indicates whether the user has an active MCP token and when it expires.
/// The hash and raw token are intentionally omitted.
/// </summary>
public sealed record GetMcpTokenStatusResult(bool HasToken, DateTime? ExpiresAt);
