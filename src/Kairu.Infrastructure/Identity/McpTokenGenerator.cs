using System.Security.Cryptography;
using System.Text;
using Kairu.Application.Identity;
using Kairu.Domain.Identity;

namespace Kairu.Infrastructure.Identity;

/// <summary>
/// Generates and hashes MCP personal tokens.
/// Token format: "kairu_" + 24 random bytes encoded as base64url (32 chars, ~160 bits of entropy).
/// Hash: SHA-256 of the raw token value, stored as 64-character lowercase hex.
/// </summary>
internal sealed class McpTokenGenerator : IMcpTokenGenerator
{
    public McpRawToken Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(24);
        var base64Url = Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        return new McpRawToken($"kairu_{base64Url}");
    }

    public McpTokenHash Hash(McpRawToken raw)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw.Value));
        var hex = Convert.ToHexString(hashBytes).ToLowerInvariant();

        // McpTokenHash.Create is guaranteed to succeed for a valid SHA-256 hex string
        var result = McpTokenHash.Create(hex);
        return result.Value;
    }
}
