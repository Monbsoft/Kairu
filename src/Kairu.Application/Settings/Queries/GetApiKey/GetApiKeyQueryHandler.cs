using Kairu.Application.Common;
using Kairu.Domain.Settings;
using Microsoft.Extensions.Logging;
using Monbsoft.BrilliantMediator.Abstractions.Queries;

namespace Kairu.Application.Settings.Queries.GetApiKey;

public sealed class GetApiKeyQueryHandler : IQueryHandler<GetApiKeyQuery, GetApiKeyResult>
{
    private readonly IApiKeyRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetApiKeyQueryHandler> _logger;

    public GetApiKeyQueryHandler(
        IApiKeyRepository repository,
        ICurrentUserService currentUserService,
        ILogger<GetApiKeyQueryHandler> logger)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<GetApiKeyResult> Handle(
        GetApiKeyQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;
        var apiKey = await _repository.GetByUserIdAsync(userId, cancellationToken);

        _logger.LogDebug("GetApiKey for user {UserId}: exists={Exists}", userId, apiKey is not null);
        return new GetApiKeyResult(apiKey is not null, apiKey?.CreatedAt);
    }
}
