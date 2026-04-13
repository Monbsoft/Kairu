using Kairu.Domain.Common;

namespace Kairu.Domain.Identity;

/// <summary>
/// Value Object representing the SHA-256 hex hash of a raw MCP token.
/// Invariant: exactly 64 lowercase hexadecimal characters.
/// </summary>
public sealed record McpTokenHash
{
    public string Value { get; }

    private McpTokenHash(string value) => Value = value;

    /// <summary>Restores a hash from a trusted stored value (EF Core / DB) — bypasses validation.</summary>
    public static McpTokenHash Restore(string storedValue) => new(storedValue);

    public static Result<McpTokenHash> Create(string sha256Hex)
    {
        if (string.IsNullOrWhiteSpace(sha256Hex))
            return Result.Failure<McpTokenHash>(McpTokenErrors.InvalidHash);

        if (sha256Hex.Length != 64)
            return Result.Failure<McpTokenHash>(McpTokenErrors.InvalidHash);

        foreach (var c in sha256Hex)
        {
            if (!IsHexChar(c))
                return Result.Failure<McpTokenHash>(McpTokenErrors.InvalidHash);
        }

        return Result.Success(new McpTokenHash(sha256Hex.ToLowerInvariant()));
    }

    private static bool IsHexChar(char c)
        => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
}
