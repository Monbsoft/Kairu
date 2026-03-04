namespace Kairudev.Domain.Identity;

public interface IUserRepository
{
    Task<User?> GetByGitHubIdAsync(string githubId, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
}
