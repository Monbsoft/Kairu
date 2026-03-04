using Kairudev.Domain.Identity;

namespace Kairudev.Application.Common;

public interface ICurrentUserService
{
    UserId CurrentUserId { get; }
}
