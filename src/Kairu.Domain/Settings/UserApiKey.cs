using Kairu.Domain.Identity;

namespace Kairu.Domain.Settings;

public sealed class UserApiKey
{
    public UserId OwnerId { get; private set; }
    public string KeyHash { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private UserApiKey() { OwnerId = null!; KeyHash = null!; }

    public static UserApiKey Create(UserId ownerId, string keyHash, DateTime createdAt)
        => new() { OwnerId = ownerId, KeyHash = keyHash, CreatedAt = createdAt };

    /// <summary>Remplace la clé existante (upsert).</summary>
    public void Regenerate(string keyHash, DateTime createdAt)
    {
        KeyHash = keyHash;
        CreatedAt = createdAt;
    }
}
