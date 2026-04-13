using Kairu.Domain.Common;
using Kairu.Domain.Identity;
using Microsoft.Extensions.Logging;
using Monbsoft.BrilliantMediator.Abstractions.Queries;

namespace Kairu.Application.Identity.Queries.GetMcpTokenStatus;

/// <summary>
/// Handles the GetMcpTokenStatus query.
/// Returns HasToken=false with ExpiresAt=null when no token exists.
/// </summary>
public sealed class GetMcpTokenStatusQueryHandler
    : IQueryHandler<GetMcpTokenStatusQuery, Result<GetMcpTokenStatusResult>>
{
    private readonly IMcpTokenRepository _repository;
    private readonly ILogger<GetMcpTokenStatusQueryHandler> _logger;

    public GetMcpTokenStatusQueryHandler(
        IMcpTokenRepository repository,
        ILogger<GetMcpTokenStatusQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<GetMcpTokenStatusResult>> Handle(
        GetMcpTokenStatusQuery query,
        CancellationToken ct = default)
    {
        _logger.LogDebug("Getting MCP token status for user {UserId}", query.UserId);

        var token = await _repository.GetByUserIdAsync(query.UserId, ct);

        if (token is null)
            return Result.Success(new GetMcpTokenStatusResult(HasToken: false, ExpiresAt: null));

        return Result.Success(new GetMcpTokenStatusResult(HasToken: true, ExpiresAt: token.ExpiresAt));
    }
}
