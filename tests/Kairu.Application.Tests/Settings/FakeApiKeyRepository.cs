using Kairu.Domain.Identity;
using Kairu.Domain.Settings;

namespace Kairu.Application.Tests.Settings;

internal sealed class FakeApiKeyRepository : IApiKeyRepository
{
    public UserApiKey? Stored { get; private set; }

    public Task UpsertAsync(UserApiKey apiKey, CancellationToken cancellationToken = default)
    {
        if (Stored is not null && Stored.OwnerId == apiKey.OwnerId)
        {
            // Simulate EfCore upsert: mutate the existing entity via Regenerate
            Stored.Regenerate(apiKey.KeyHash, apiKey.CreatedAt);
        }
        else
        {
            Stored = apiKey;
        }
        return Task.CompletedTask;
    }

    public Task<UserId?> GetUserIdByHashAsync(string keyHash, CancellationToken cancellationToken = default)
    {
        if (Stored is not null && Stored.KeyHash == keyHash)
            return Task.FromResult<UserId?>(Stored.OwnerId);
        return Task.FromResult<UserId?>(null);
    }

    public Task<UserApiKey?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        if (Stored is not null && Stored.OwnerId == userId)
            return Task.FromResult<UserApiKey?>(Stored);
        return Task.FromResult<UserApiKey?>(null);
    }

    public Task DeleteByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        if (Stored?.OwnerId == userId) Stored = null;
        return Task.CompletedTask;
    }
}
