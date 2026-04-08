using System.Collections.Concurrent;
using Kairu.Application.OAuth;

namespace Kairu.Infrastructure.OAuth;

public sealed class InMemoryAuthorizationCodeStore : IAuthorizationCodeStore
{
    private readonly ConcurrentDictionary<string, AuthorizationCodeEntry> _codes = new();
    private DateTime _lastPurge = DateTime.UtcNow;
    private static readonly TimeSpan PurgeInterval = TimeSpan.FromMinutes(10);

    public Task StoreAsync(string code, AuthorizationCodeEntry entry, CancellationToken cancellationToken = default)
    {
        _codes[code] = entry;
        PurgeExpiredIfNeeded();
        return Task.CompletedTask;
    }

    public Task<AuthorizationCodeEntry?> ConsumeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (!_codes.TryRemove(code, out var entry))
            return Task.FromResult<AuthorizationCodeEntry?>(null);

        // Expired codes are treated as non-existent
        if (entry.ExpiresAt <= DateTime.UtcNow)
            return Task.FromResult<AuthorizationCodeEntry?>(null);

        return Task.FromResult<AuthorizationCodeEntry?>(entry);
    }

    /// <summary>
    /// Passive purge: removes all expired entries every 10 minutes.
    /// Called on StoreAsync to avoid accumulating abandoned authorization codes.
    /// </summary>
    private void PurgeExpiredIfNeeded()
    {
        var now = DateTime.UtcNow;
        if (now - _lastPurge < PurgeInterval) return;

        _lastPurge = now;
        foreach (var kvp in _codes)
        {
            if (kvp.Value.ExpiresAt <= now)
                _codes.TryRemove(kvp.Key, out _);
        }
    }
}
