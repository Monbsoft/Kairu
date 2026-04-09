using Kairu.Domain.Common;
using Kairu.Domain.Identity;
using Microsoft.Extensions.Logging;
using Monbsoft.BrilliantMediator.Abstractions.Queries;

namespace Kairu.Application.Identity.Queries.ValidateMcpToken;

/// <summary>
/// Handles the ValidateMcpToken query.
/// Steps:
///   1. Hash the raw token.
///   2. Look up the token by hash.
///   3. Return Failure(NoTokenFound) if not found.
///   4. Return Failure(Expired) if ExpiresAt &lt;= UtcNow.
///   5. Return the associated UserId.
/// </summary>
public sealed class ValidateMcpTokenQueryHandler
    : IQueryHandler<ValidateMcpTokenQuery, Result<ValidateMcpTokenResult>>
{
    private readonly IMcpTokenRepository _repository;
    private readonly IMcpTokenGenerator _generator;
    private readonly ILogger<ValidateMcpTokenQueryHandler> _logger;

    public ValidateMcpTokenQueryHandler(
        IMcpTokenRepository repository,
        IMcpTokenGenerator generator,
        ILogger<ValidateMcpTokenQueryHandler> logger)
    {
        _repository = repository;
        _generator = generator;
        _logger = logger;
    }

    public async Task<Result<ValidateMcpTokenResult>> Handle(
        ValidateMcpTokenQuery query,
        CancellationToken ct = default)
    {
        var hash = _generator.Hash(query.RawToken);

        var token = await _repository.GetByHashAsync(hash, ct);
        if (token is null)
        {
            _logger.LogWarning("MCP token validation failed: no token found for provided hash");
            return Result.Failure<ValidateMcpTokenResult>(McpTokenErrors.NoTokenFound);
        }

        if (token.IsExpired(DateTime.UtcNow))
        {
            _logger.LogWarning("MCP token validation failed: token {TokenId} expired at {ExpiresAt}", token.Id, token.ExpiresAt);
            return Result.Failure<ValidateMcpTokenResult>(McpTokenErrors.Expired);
        }

        _logger.LogDebug("MCP token validated for user {UserId}", token.UserId);
        return Result.Success(new ValidateMcpTokenResult(token.UserId));
    }
}
