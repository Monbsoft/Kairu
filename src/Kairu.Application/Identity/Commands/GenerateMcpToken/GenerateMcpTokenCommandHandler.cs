using Kairu.Domain.Common;
using Kairu.Domain.Identity;
using Microsoft.Extensions.Logging;
using Monbsoft.BrilliantMediator.Abstractions.Commands;

namespace Kairu.Application.Identity.Commands.GenerateMcpToken;

/// <summary>
/// Handles the GenerateMcpToken command.
/// Steps:
///   1. Delete any existing token for the user.
///   2. Generate a new raw token via IMcpTokenGenerator.
///   3. Compute its hash.
///   4. Create and persist the McpToken entity.
///   5. Return the raw token (one-time only).
/// </summary>
public sealed class GenerateMcpTokenCommandHandler
    : ICommandHandler<GenerateMcpTokenCommand, Result<GenerateMcpTokenResult>>
{
    private readonly IMcpTokenRepository _repository;
    private readonly IMcpTokenGenerator _generator;
    private readonly ILogger<GenerateMcpTokenCommandHandler> _logger;

    public GenerateMcpTokenCommandHandler(
        IMcpTokenRepository repository,
        IMcpTokenGenerator generator,
        ILogger<GenerateMcpTokenCommandHandler> logger)
    {
        _repository = repository;
        _generator = generator;
        _logger = logger;
    }

    public async Task<Result<GenerateMcpTokenResult>> Handle(
        GenerateMcpTokenCommand command,
        CancellationToken ct = default)
    {
        _logger.LogDebug("Generating MCP token for user {UserId}", command.UserId);

        // Step 1: revoke existing token if any
        await _repository.DeleteByUserIdAsync(command.UserId, ct);

        // Step 2 & 3: generate raw token and hash
        var rawToken = _generator.Generate();
        var hash = _generator.Hash(rawToken);

        // Step 4: create entity (1 year expiry)
        var now = DateTime.UtcNow;
        var expiresAt = now.AddYears(1);
        var token = McpToken.Create(command.UserId, hash, expiresAt, now);

        await _repository.AddAsync(token, ct);

        _logger.LogInformation("MCP token {TokenId} generated for user {UserId}, expires {ExpiresAt}",
            token.Id, command.UserId, expiresAt);

        // Step 5: return raw token (one-time only)
        return Result.Success(new GenerateMcpTokenResult(rawToken, expiresAt));
    }
}
