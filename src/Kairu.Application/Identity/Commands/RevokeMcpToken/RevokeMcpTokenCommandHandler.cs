using Kairu.Domain.Common;
using Kairu.Domain.Identity;
using Microsoft.Extensions.Logging;
using Monbsoft.BrilliantMediator.Abstractions.Commands;

namespace Kairu.Application.Identity.Commands.RevokeMcpToken;

/// <summary>
/// Handles the RevokeMcpToken command.
/// Steps:
///   1. Check that an active token exists for the user.
///   2. Delete it.
///   3. Return Success or Failure(NoTokenFound).
/// </summary>
public sealed class RevokeMcpTokenCommandHandler
    : ICommandHandler<RevokeMcpTokenCommand, Result<RevokeMcpTokenResult>>
{
    private readonly IMcpTokenRepository _repository;
    private readonly ILogger<RevokeMcpTokenCommandHandler> _logger;

    public RevokeMcpTokenCommandHandler(
        IMcpTokenRepository repository,
        ILogger<RevokeMcpTokenCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<RevokeMcpTokenResult>> Handle(
        RevokeMcpTokenCommand command,
        CancellationToken ct = default)
    {
        _logger.LogDebug("Revoking MCP token for user {UserId}", command.UserId);

        var existing = await _repository.GetByUserIdAsync(command.UserId, ct);
        if (existing is null)
        {
            _logger.LogWarning("No MCP token found for user {UserId}", command.UserId);
            return Result.Failure<RevokeMcpTokenResult>(McpTokenErrors.NoTokenFound);
        }

        await _repository.DeleteByUserIdAsync(command.UserId, ct);

        _logger.LogInformation("MCP token revoked for user {UserId}", command.UserId);

        return Result.Success(new RevokeMcpTokenResult());
    }
}
