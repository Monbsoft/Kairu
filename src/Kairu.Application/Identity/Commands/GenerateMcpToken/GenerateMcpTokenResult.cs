using Kairu.Domain.Identity;

namespace Kairu.Application.Identity.Commands.GenerateMcpToken;

/// <summary>
/// Contains the raw token returned once upon generation, and the expiry date.
/// The raw token must be displayed to the user immediately — it cannot be retrieved later.
/// </summary>
public sealed record GenerateMcpTokenResult(McpRawToken RawToken, DateTime ExpiresAt);
