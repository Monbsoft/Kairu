using Kairudev.Domain.Identity;

namespace Kairudev.Application.Identity.Commands.GetOrCreateUser;

public sealed record GetOrCreateUserResult(UserId UserId, string Login, string DisplayName);
