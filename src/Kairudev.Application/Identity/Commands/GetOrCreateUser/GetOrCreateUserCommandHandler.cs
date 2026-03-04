using Kairudev.Domain.Common;
using Kairudev.Domain.Identity;

namespace Kairudev.Application.Identity.Commands.GetOrCreateUser;

public sealed class GetOrCreateUserCommandHandler
{
    private readonly IUserRepository _userRepository;

    public GetOrCreateUserCommandHandler(IUserRepository userRepository)
        => _userRepository = userRepository;

    public async Task<Result<GetOrCreateUserResult>> Handle(
        GetOrCreateUserCommand command, CancellationToken ct = default)
    {
        var existing = await _userRepository.GetByGitHubIdAsync(command.GitHubId, ct);
        if (existing is not null)
            return Result.Success(new GetOrCreateUserResult(existing.Id, existing.Login, existing.DisplayName));

        var user = User.Create(command.GitHubId, command.Login, command.DisplayName, command.Email);
        await _userRepository.AddAsync(user, ct);
        return Result.Success(new GetOrCreateUserResult(user.Id, user.Login, user.DisplayName));
    }
}
