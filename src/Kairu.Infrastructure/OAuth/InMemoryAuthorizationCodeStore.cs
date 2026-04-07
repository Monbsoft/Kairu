using System.Collections.Concurrent;
using Kairu.Application.OAuth;

namespace Kairu.Infrastructure.OAuth;

public sealed class InMemoryAuthorizationCodeStore : IAuthorizationCodeStore
{
    private readonly ConcurrentDictionary<string, AuthorizationCodeEntry> _codes = new();

    public Task StoreAsync(string code, AuthorizationCodeEntry entry, CancellationToken cancellationToken = default)
    {
        _codes[code] = entry;
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
}
