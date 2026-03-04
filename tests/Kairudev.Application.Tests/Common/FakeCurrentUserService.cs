using Kairudev.Application.Common;
using Kairudev.Domain.Identity;

namespace Kairudev.Application.Tests.Common;

internal sealed class FakeCurrentUserService : ICurrentUserService
{
    public UserId CurrentUserId => UserId.From("test-github-id-123");
}
