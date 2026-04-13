using Kairu.Domain.Identity;

namespace Kairu.Application.Tests.Identity;

internal sealed class FakeMcpTokenRepository : IMcpTokenRepository
{
    public List<McpToken> Tokens { get; } = [];

    public Task<McpToken?> GetByUserIdAsync(UserId userId, CancellationToken ct = default)
        => Task.FromResult(Tokens.FirstOrDefault(t => t.UserId == userId));

    public Task<McpToken?> GetByHashAsync(McpTokenHash hash, CancellationToken ct = default)
        => Task.FromResult(Tokens.FirstOrDefault(t => t.TokenHash == hash));

    public Task AddAsync(McpToken token, CancellationToken ct = default)
    {
        Tokens.Add(token);
        return Task.CompletedTask;
    }

    public Task DeleteByUserIdAsync(UserId userId, CancellationToken ct = default)
    {
        Tokens.RemoveAll(t => t.UserId == userId);
        return Task.CompletedTask;
    }

    public Task ReplaceAsync(McpToken newToken, CancellationToken ct = default)
    {
        Tokens.RemoveAll(t => t.UserId == newToken.UserId);
        Tokens.Add(newToken);
        return Task.CompletedTask;
    }
}
