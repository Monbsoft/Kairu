using Kairu.Domain.Common;

namespace Kairu.Domain.Identity;

/// <summary>
/// Aggregate: personal MCP authentication token.
/// At most one active token per user (enforced by a unique DB constraint on UserId).
/// The raw token is never stored — only its SHA-256 hex hash is persisted.
/// </summary>
public sealed class McpToken : Entity<McpTokenId>
{
    public UserId UserId { get; private set; } = default!;
    public McpTokenHash TokenHash { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    // For EF Core materialization
    private McpToken() : base() { }

    private McpToken(McpTokenId id, UserId userId, McpTokenHash tokenHash, DateTime createdAt, DateTime expiresAt)
        : base(id)
    {
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    /// <summary>
    /// Creates a new MCP token entity.
    /// </summary>
    /// <param name="userId">Owner of the token.</param>
    /// <param name="tokenHash">SHA-256 hex hash of the raw token.</param>
    /// <param name="expiresAt">Expiry date (must be in the future relative to createdAt).</param>
    /// <param name="createdAt">Creation timestamp in UTC. Defaults to UtcNow.</param>
    public static McpToken Create(UserId userId, McpTokenHash tokenHash, DateTime expiresAt, DateTime? createdAt = null)
    {
        var now = createdAt ?? DateTime.UtcNow;
        return new McpToken(McpTokenId.New(), userId, tokenHash, now, expiresAt);
    }

    public bool IsExpired(DateTime utcNow) => ExpiresAt <= utcNow;
}
