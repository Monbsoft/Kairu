using System.Security.Claims;
using Kairudev.Application.Common;
using Kairudev.Domain.Identity;

namespace Kairudev.Api.Auth;

public sealed class ClaimsCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClaimsCurrentUserService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public UserId CurrentUserId
    {
        get
        {
            var sub = _httpContextAccessor.HttpContext?.User
                .FindFirst("sub")?.Value
                ?? _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(sub))
                throw new UnauthorizedAccessException("User is not authenticated.");

            return UserId.From(sub);
        }
    }
}
