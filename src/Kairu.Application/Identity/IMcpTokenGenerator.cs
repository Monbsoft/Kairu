using Kairu.Domain.Identity;

namespace Kairu.Application.Identity;

/// <summary>
/// Generates raw MCP tokens and computes their SHA-256 hash.
/// Implemented in Kairu.Infrastructure.Identity.
/// </summary>
public interface IMcpTokenGenerator
{
    /// <summary>
    /// Generates a new random raw token in the form "kairu_&lt;base64url(24 bytes)&gt;".
    /// </summary>
    McpRawToken Generate();

    /// <summary>
    /// Computes the SHA-256 hex hash of the given raw token.
    /// </summary>
    McpTokenHash Hash(McpRawToken rawToken);
}
