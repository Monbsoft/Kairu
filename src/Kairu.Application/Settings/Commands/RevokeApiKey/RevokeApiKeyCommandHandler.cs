using Kairu.Application.Common;
using Kairu.Domain.Settings;
using Microsoft.Extensions.Logging;
using Monbsoft.BrilliantMediator.Abstractions.Commands;

namespace Kairu.Application.Settings.Commands.RevokeApiKey;

public sealed class RevokeApiKeyCommandHandler
    : ICommandHandler<RevokeApiKeyCommand, RevokeApiKeyResult>
{
    private readonly IApiKeyRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RevokeApiKeyCommandHandler> _logger;

    public RevokeApiKeyCommandHandler(
        IApiKeyRepository repository,
        ICurrentUserService currentUserService,
        ILogger<RevokeApiKeyCommandHandler> logger)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<RevokeApiKeyResult> Handle(
        RevokeApiKeyCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;
        await _repository.DeleteByUserIdAsync(userId, cancellationToken);
        _logger.LogInformation("API Key revoked for user {UserId}", userId);
        return RevokeApiKeyResult.Success();
    }
}
