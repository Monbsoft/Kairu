using System.Security.Cryptography;
using System.Text;
using Kairu.Application.Common;
using Kairu.Domain.Settings;
using Microsoft.Extensions.Logging;
using Monbsoft.BrilliantMediator.Abstractions.Commands;

namespace Kairu.Application.Settings.Commands.GenerateApiKey;

public sealed class GenerateApiKeyCommandHandler
    : ICommandHandler<GenerateApiKeyCommand, GenerateApiKeyResult>
{
    private readonly IApiKeyRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GenerateApiKeyCommandHandler> _logger;

    public GenerateApiKeyCommandHandler(
        IApiKeyRepository repository,
        ICurrentUserService currentUserService,
        ILogger<GenerateApiKeyCommandHandler> logger)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<GenerateApiKeyResult> Handle(
        GenerateApiKeyCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;

        // Génère un token URL-safe : "kairu_" + base64url(32 bytes aléatoires)
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token = "kairu_" + Convert.ToBase64String(tokenBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        // Hash SHA-256 — seul ce hash est persisté
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        var keyHash = Convert.ToHexString(hashBytes).ToLowerInvariant();

        var apiKey = UserApiKey.Create(userId, keyHash, DateTime.UtcNow);
        await _repository.UpsertAsync(apiKey, cancellationToken);

        _logger.LogInformation("API Key generated for user {UserId}", userId);
        return GenerateApiKeyResult.Success(token);
    }
}
