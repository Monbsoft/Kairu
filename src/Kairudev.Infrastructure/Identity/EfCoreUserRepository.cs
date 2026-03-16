using Kairudev.Domain.Identity;
using Kairudev.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kairudev.Infrastructure.Identity;

internal sealed class EfCoreUserRepository : IUserRepository
{
    private readonly KairudevDbContext _context;

    public EfCoreUserRepository(KairudevDbContext context) => _context = context;

    public async Task<User?> GetByGitHubIdAsync(string githubId, CancellationToken ct = default)
        => await _context.Users.FirstOrDefaultAsync(u => u.GitHubId == githubId, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(user, ct);
        await _context.SaveChangesAsync(ct);
    }
}
