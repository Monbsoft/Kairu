namespace Kairu.Domain.Identity;

public interface IMcpTokenRepository
{
    Task<McpToken?> GetByUserIdAsync(UserId userId, CancellationToken ct = default);
    Task<McpToken?> GetByHashAsync(McpTokenHash hash, CancellationToken ct = default);
    Task AddAsync(McpToken token, CancellationToken ct = default);
    Task DeleteByUserIdAsync(UserId userId, CancellationToken ct = default);
}
