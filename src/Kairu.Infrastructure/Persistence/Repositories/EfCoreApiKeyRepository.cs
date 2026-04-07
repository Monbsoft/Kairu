using Kairu.Domain.Identity;
using Kairu.Domain.Settings;
using Microsoft.EntityFrameworkCore;

namespace Kairu.Infrastructure.Persistence.Repositories;

internal sealed class EfCoreApiKeyRepository : IApiKeyRepository
{
    private readonly KairuDbContext _db;

    public EfCoreApiKeyRepository(KairuDbContext db) => _db = db;

    public async Task UpsertAsync(UserApiKey apiKey, CancellationToken cancellationToken = default)
    {
        var existing = await _db.UserApiKeys
            .FirstOrDefaultAsync(k => k.Id == apiKey.Id, cancellationToken);

        if (existing is null)
            _db.UserApiKeys.Add(apiKey);
        else
            existing.Regenerate(apiKey.KeyHash, apiKey.CreatedAt);

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserId?> GetUserIdByHashAsync(
        string keyHash, CancellationToken cancellationToken = default)
    {
        var apiKey = await _db.UserApiKeys
            .FirstOrDefaultAsync(k => k.KeyHash == keyHash, cancellationToken);
        return apiKey?.Id;
    }

    public async Task<UserApiKey?> GetByUserIdAsync(
        UserId userId, CancellationToken cancellationToken = default)
        => await _db.UserApiKeys
            .FirstOrDefaultAsync(k => k.Id == userId, cancellationToken);

    public async Task DeleteByUserIdAsync(
        UserId userId, CancellationToken cancellationToken = default)
    {
        var apiKey = await _db.UserApiKeys
            .FirstOrDefaultAsync(k => k.Id == userId, cancellationToken);
        if (apiKey is not null)
        {
            _db.UserApiKeys.Remove(apiKey);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
