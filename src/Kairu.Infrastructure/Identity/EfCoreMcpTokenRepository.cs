using Kairu.Domain.Identity;
using Kairu.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kairu.Infrastructure.Identity;

internal sealed class EfCoreMcpTokenRepository : IMcpTokenRepository
{
    private readonly KairuDbContext _context;

    public EfCoreMcpTokenRepository(KairuDbContext context) => _context = context;

    public async Task<McpToken?> GetByUserIdAsync(UserId userId, CancellationToken ct = default)
        => await _context.McpTokens
            .FirstOrDefaultAsync(t => t.UserId == userId, ct);

    public async Task<McpToken?> GetByHashAsync(McpTokenHash hash, CancellationToken ct = default)
        => await _context.McpTokens
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

    public async Task AddAsync(McpToken token, CancellationToken ct = default)
    {
        await _context.McpTokens.AddAsync(token, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteByUserIdAsync(UserId userId, CancellationToken ct = default)
    {
        var token = await _context.McpTokens
            .FirstOrDefaultAsync(t => t.UserId == userId, ct);

        if (token is not null)
        {
            _context.McpTokens.Remove(token);
            await _context.SaveChangesAsync(ct);
        }
    }
}
