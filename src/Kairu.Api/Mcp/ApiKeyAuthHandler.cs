using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Kairu.Domain.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Kairu.Api.Mcp;

public sealed class ApiKeyAuthHandler : AuthenticationHandler<ApiKeyAuthOptions>
{
    private readonly IApiKeyRepository _apiKeyRepository;

    public ApiKeyAuthHandler(
        IOptionsMonitor<ApiKeyAuthOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiKeyRepository apiKeyRepository)
        : base(options, logger, encoder)
    {
        _apiKeyRepository = apiKeyRepository;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Extraire le token depuis Authorization: Bearer kairu_xxx
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            return AuthenticateResult.NoResult();

        var headerValue = authHeader.ToString();
        if (!headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var token = headerValue["Bearer ".Length..].Trim();

        if (string.IsNullOrEmpty(token) || !token.StartsWith("kairu_"))
            return AuthenticateResult.Fail("Invalid API key format.");

        // Hash SHA-256 du token (même normalisation que GenerateApiKeyCommandHandler)
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        var keyHash = Convert.ToHexString(hashBytes).ToLowerInvariant();

        // Lookup en base
        var userId = await _apiKeyRepository.GetUserIdByHashAsync(keyHash, Context.RequestAborted);
        if (userId is null)
            return AuthenticateResult.Fail("Invalid or revoked API key.");

        // ClaimsPrincipal identique au flux JWT (claim "sub" = UserId GUID)
        var claims = new[] { new Claim("sub", userId.Value.ToString()) };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
