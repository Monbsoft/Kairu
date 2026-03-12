using Kairudev.Domain.Common;

namespace Kairudev.Domain.Identity;

public sealed class User : Entity<UserId>
{
    public string GitHubId { get; private set; } = default!;
    public string Login { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public string? Email { get; private set; }

    // For EF Core materialization
    private User() : base() { }

    private User(UserId id, string githubId, string login, string displayName, string? email)
        : base(id)
    {
        GitHubId = githubId;
        Login = login;
        DisplayName = displayName;
        Email = email;
    }

    public static User Create(string githubId, string login, string displayName, string? email)
        => new(UserId.From(githubId), githubId, login, displayName, email);
}
