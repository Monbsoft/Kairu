using Kairudev.Application.Common;
using Kairudev.Domain.Identity;

namespace Kairudev.Application.Tests.Common;

internal sealed class FakeCurrentUserService : ICurrentUserService
{
    public static readonly UserId TestUserId = UserId.From(new Guid("00000000-0000-0000-0000-000000000001"));
    public UserId CurrentUserId => TestUserId;
}
