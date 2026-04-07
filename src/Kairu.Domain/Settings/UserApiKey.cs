using Kairu.Domain.Common;
using Kairu.Domain.Identity;

namespace Kairu.Domain.Settings;

/// <summary>
/// Entity representing a hashed API key for a user. One key per user (upsert semantics).
/// </summary>
public sealed class UserApiKey : Entity<UserId>
{
    public string KeyHash { get; private set; }
    public DateTime CreatedAt { get; private set; }

    /// <summary>Convenience alias for the entity identity.</summary>
    public UserId OwnerId => Id;

    // Parameterless constructor required by EF Core for materialization
    private UserApiKey() : base(null!) { KeyHash = null!; }

    private UserApiKey(UserId ownerId, string keyHash, DateTime createdAt) : base(ownerId)
    {
        KeyHash = keyHash;
        CreatedAt = createdAt;
    }

    /// <summary>Creates a new API key entry for a user.</summary>
    public static UserApiKey Create(UserId ownerId, string keyHash, DateTime createdAt)
    {
        ArgumentNullException.ThrowIfNull(ownerId);
        if (string.IsNullOrWhiteSpace(keyHash))
            throw new ArgumentException("KeyHash cannot be empty.", nameof(keyHash));
        return new UserApiKey(ownerId, keyHash, createdAt);
    }

    /// <summary>Replaces the stored key hash (used for key rotation/regeneration).</summary>
    public void Regenerate(string keyHash, DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(keyHash))
            throw new ArgumentException("KeyHash cannot be empty.", nameof(keyHash));
        KeyHash = keyHash;
        CreatedAt = createdAt;
    }
}
