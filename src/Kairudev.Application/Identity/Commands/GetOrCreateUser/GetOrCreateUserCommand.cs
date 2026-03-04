namespace Kairudev.Application.Identity.Commands.GetOrCreateUser;

public sealed record GetOrCreateUserCommand(
    string GitHubId,
    string Login,
    string DisplayName,
    string? Email);
