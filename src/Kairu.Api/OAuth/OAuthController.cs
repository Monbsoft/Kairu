using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Kairu.Api.Auth;

using Kairu.Application.Identity.Commands.GetOrCreateUser;
using Kairu.Application.OAuth;
using Kairu.Domain.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Monbsoft.BrilliantMediator.Abstractions;

namespace Kairu.Api.OAuth;

[ApiController]
[AllowAnonymous]
public sealed class OAuthController : ControllerBase
{
    private const string AcceptedClientId = "kairu-mcp";
    private const string OAuthStateCookieName = ".Kairu.OAuthState";
    private const string DataProtectionPurpose = "Kairu.OAuth.State";
    private const int AuthorizationCodeTtlMinutes = 5;

    private readonly IAuthorizationCodeStore _codeStore;
    private readonly IMediator _mediator;
    private readonly JwtTokenService _jwtTokenService;
    private readonly IDataProtector _protector;
    private readonly ILogger<OAuthController> _logger;

    public OAuthController(
        IAuthorizationCodeStore codeStore,
        IMediator mediator,
        JwtTokenService jwtTokenService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<OAuthController> logger)
    {
        _codeStore = codeStore;
        _mediator = mediator;
        _jwtTokenService = jwtTokenService;
        _protector = dataProtectionProvider.CreateProtector(DataProtectionPurpose);
        _logger = logger;
    }

    /// <summary>
    /// RFC 8414 — Authorization Server Metadata.
    /// Allows MCP clients to discover authorize + token endpoints.
    /// </summary>
    [HttpGet("/.well-known/oauth-authorization-server")]
    public IActionResult GetAuthServerMetadata()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        return Ok(new
        {
            issuer = baseUrl,
            authorization_endpoint = $"{baseUrl}/oauth/authorize",
            token_endpoint = $"{baseUrl}/oauth/token",
            response_types_supported = new[] { "code" },
            grant_types_supported = new[] { "authorization_code" },
            code_challenge_methods_supported = new[] { "S256" },
            token_endpoint_auth_methods_supported = new[] { "none" },
        });
    }

    /// <summary>
    /// OAuth 2.1 Authorization Endpoint.
    /// Validates PKCE params, stores state in encrypted cookie, challenges GitHub OAuth.
    /// </summary>
    [HttpGet("/oauth/authorize")]
    public IActionResult Authorize(
        [FromQuery] string? client_id,
        [FromQuery] string? redirect_uri,
        [FromQuery] string? response_type,
        [FromQuery] string? code_challenge,
        [FromQuery] string? code_challenge_method,
        [FromQuery] string? state,
        [FromQuery] string? scope)
    {
        // Validate required params
        if (client_id != AcceptedClientId)
            return BadRequest(new { error = "invalid_client" });

        if (response_type != "code")
            return BadRequest(new { error = "unsupported_response_type" });

        if (string.IsNullOrEmpty(code_challenge) || code_challenge_method != "S256")
            return BadRequest(new { error = "invalid_request", error_description = "PKCE S256 is required." });

        if (string.IsNullOrEmpty(redirect_uri))
            return BadRequest(new { error = "invalid_request", error_description = "redirect_uri is required." });

        // Store PKCE state in encrypted cookie (survives GitHub redirect)
        var statePayload = JsonSerializer.Serialize(new OAuthStateCookie(code_challenge, redirect_uri, state ?? string.Empty));
        var encrypted = _protector.Protect(statePayload);

        Response.Cookies.Append(OAuthStateCookieName, encrypted, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            MaxAge = TimeSpan.FromMinutes(10),
            Path = "/oauth"
        });

        // Challenge GitHub OAuth — redirect back to our MCP callback
        var callbackUrl = Url.Action(nameof(GitHubCallback), "OAuth", null, Request.Scheme);
        var properties = new AuthenticationProperties { RedirectUri = callbackUrl };
        return Challenge(properties, "GitHub");
    }

    /// <summary>
    /// Internal GitHub OAuth callback for MCP flow.
    /// Reads cookie state, creates user, generates authorization code, redirects to MCP client.
    /// </summary>
    [HttpGet("/oauth/github/callback")]
    public async Task<IActionResult> GitHubCallback(CancellationToken ct)
    {
        // Read and clear encrypted state cookie
        if (!Request.Cookies.TryGetValue(OAuthStateCookieName, out var encrypted) || string.IsNullOrEmpty(encrypted))
        {
            _logger.LogWarning("OAuth callback: missing state cookie");
            return BadRequest(new { error = "invalid_request", error_description = "Missing OAuth state." });
        }

        Response.Cookies.Delete(OAuthStateCookieName, new CookieOptions { Path = "/oauth" });

        OAuthStateCookie? cookieState;
        try
        {
            var decrypted = _protector.Unprotect(encrypted);
            cookieState = JsonSerializer.Deserialize<OAuthStateCookie>(decrypted);
            if (cookieState is null) throw new FormatException();
        }
        catch
        {
            return BadRequest(new { error = "invalid_request", error_description = "Corrupted OAuth state." });
        }

        var (codeChallenge, redirectUri, state) = cookieState;

        // Authenticate via GitHub cookie
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!result.Succeeded)
        {
            _logger.LogWarning("OAuth callback: GitHub auth failed: {Error}", result.Failure?.Message);
            return BadRequest(new { error = "access_denied" });
        }

        var githubId = result.Principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var login = result.Principal.FindFirst("urn:github:login")?.Value
            ?? result.Principal.FindFirst(ClaimTypes.Name)?.Value ?? "unknown";
        var displayName = result.Principal.FindFirst(ClaimTypes.Name)?.Value ?? login;
        var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(githubId))
            return BadRequest(new { error = "access_denied", error_description = "No GitHub ID." });

        // Get or create Kairu user
        var command = new GetOrCreateUserCommand(githubId, login, displayName, email);
        var userResult = await _mediator.DispatchAsync<GetOrCreateUserCommand, Result<GetOrCreateUserResult>>(command, ct);

        if (userResult.IsFailure)
        {
            _logger.LogError("OAuth callback: user creation failed: {Error}", userResult.Error);
            return BadRequest(new { error = "server_error" });
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Generate authorization code (cryptographically random, single-use)
        var codeBytes = RandomNumberGenerator.GetBytes(32);
        var code = Convert.ToBase64String(codeBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');

        var entry = new AuthorizationCodeEntry(
            userResult.Value.UserId,
            userResult.Value.DisplayName,
            userResult.Value.Login,
            codeChallenge,
            redirectUri,
            DateTime.UtcNow.AddMinutes(AuthorizationCodeTtlMinutes));

        await _codeStore.StoreAsync(code, entry, ct);

        // Redirect to MCP client with authorization code
        var separator = redirectUri.Contains('?') ? "&" : "?";
        var redirectUrl = $"{redirectUri}{separator}code={Uri.EscapeDataString(code)}";
        if (!string.IsNullOrEmpty(state))
            redirectUrl += $"&state={Uri.EscapeDataString(state)}";

        return Redirect(redirectUrl);
    }

    /// <summary>
    /// OAuth 2.1 Token Endpoint.
    /// Exchanges authorization code + PKCE verifier for a JWT access token.
    /// </summary>
    [HttpPost("/oauth/token")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Token(
        [FromForm] string? grant_type,
        [FromForm] string? code,
        [FromForm] string? code_verifier,
        [FromForm] string? redirect_uri,
        [FromForm] string? client_id,
        CancellationToken ct)
    {
        if (grant_type != "authorization_code")
            return BadRequest(new { error = "unsupported_grant_type" });

        if (client_id != AcceptedClientId)
            return BadRequest(new { error = "invalid_client" });

        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(code_verifier) || string.IsNullOrEmpty(redirect_uri))
            return BadRequest(new { error = "invalid_request" });

        // Consume code (single-use)
        var entry = await _codeStore.ConsumeAsync(code, ct);
        if (entry is null)
            return BadRequest(new { error = "invalid_grant" });

        // Verify redirect_uri matches
        if (!string.Equals(entry.RedirectUri, redirect_uri, StringComparison.Ordinal))
            return BadRequest(new { error = "invalid_grant" });

        // Verify PKCE: BASE64URL(SHA256(code_verifier)) == code_challenge
        var challengeBytes = SHA256.HashData(Encoding.ASCII.GetBytes(code_verifier));
        var computedChallenge = Convert.ToBase64String(challengeBytes)
            .Replace("+", "-").Replace("/", "_").TrimEnd('=');

        if (!string.Equals(computedChallenge, entry.CodeChallenge, StringComparison.Ordinal))
            return BadRequest(new { error = "invalid_grant" });

        // Generate JWT (same claims as Web flow: sub + name + login)
        var userResult = new GetOrCreateUserResult(entry.UserId, entry.DisplayName, entry.Login);
        var token = _jwtTokenService.GenerateToken(userResult);

        return Ok(new
        {
            access_token = token,
            token_type = "Bearer",
            expires_in = _jwtTokenService.ExpiryHours * 3600
        });
    }

    private sealed record OAuthStateCookie(string CodeChallenge, string RedirectUri, string State);
}
