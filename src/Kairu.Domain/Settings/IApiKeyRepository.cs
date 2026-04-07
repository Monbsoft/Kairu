using Kairu.Domain.Identity;

namespace Kairu.Domain.Settings;

public interface IApiKeyRepository
{
    /// <summary>Upsert la clé pour un utilisateur (une seule clé active par user).</summary>
    Task UpsertAsync(UserApiKey apiKey, CancellationToken cancellationToken = default);

    /// <summary>Retourne le UserId associé à ce hash, ou null si inconnu.</summary>
    Task<UserId?> GetUserIdByHashAsync(string keyHash, CancellationToken cancellationToken = default);

    /// <summary>Retourne la clé de l'utilisateur, ou null si absente.</summary>
    Task<UserApiKey?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>Supprime la clé de l'utilisateur. Idempotent.</summary>
    Task DeleteByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
}
