namespace Kairu.Domain.Identity;

/// <summary>
/// Represents the raw (plaintext) MCP token.
/// This value object is never persisted — it exists only in memory for the duration of a single request.
/// It is returned once upon token generation and must be displayed to the user immediately.
/// </summary>
public sealed record McpRawToken(string Value);
