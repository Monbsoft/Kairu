using System.Security.Claims;
using KairuFocus.Application.Identity.Commands.GetOrCreateUser;
using KairuFocus.Domain.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monbsoft.BrilliantMediator.Abstractions;

namespace KairuFocus.Api.Auth;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly JwtTokenService _jwtTokenService;
    private readonly ILogger<AuthController> _logger;
    private readonly IReadOnlySet<string> _allowedCallbackUrls;

    public AuthController(
        IMediator mediator,
        IConfiguration configuration,
        JwtTokenService jwtTokenService,
        ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _configuration = configuration;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
        _allowedCallbackUrls = new HashSet<string>(
            configuration.GetSection("AllowedCallbackUrls").Get<string[]>() ?? [],
            StringComparer.Ordinal);
    }

    [HttpGet("github")]
    [AllowAnonymous]
    public IActionResult GitHubLogin([FromQuery] string? returnUrl = null)
    {
        var callbackUrl = Url.Action(nameof(GitHubCallback), "Auth",
            returnUrl is not null ? new { returnUrl } : null,
            Request.Scheme);

        var properties = new AuthenticationProperties { RedirectUri = callbackUrl };
        return Challenge(properties, "GitHub");
    }

    [HttpGet("github/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> GitHubCallback(
        [FromQuery] string? returnUrl = null,
        CancellationToken ct = default)
    {
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var webBase = _configuration["WebBaseUrl"] ?? "http://localhost:5010";

        if (!result.Succeeded)
        {
            _logger.LogWarning("GitHub authentication failed: {Error}", result.Failure?.Message);
            return await RedirectWithErrorAsync(webBase, "denied");
        }

        var githubId = result.Principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var login = result.Principal.FindFirst("urn:github:login")?.Value
            ?? result.Principal.FindFirst(ClaimTypes.Name)?.Value ?? "unknown";
        var displayName = result.Principal.FindFirst(ClaimTypes.Name)?.Value ?? login;
        var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(githubId))
            return await RedirectWithErrorAsync(webBase, "no-id");

        var command = new GetOrCreateUserCommand(githubId, login, displayName, email);
        var userResult = await _mediator.DispatchAsync<GetOrCreateUserCommand, Result<GetOrCreateUserResult>>(command, ct);

        if (userResult.IsFailure)
        {
            _logger.LogError("Failed to get or create user: {Error}", userResult.Error);
            return await RedirectWithErrorAsync(webBase, "server");
        }

        var token = _jwtTokenService.GenerateToken(userResult.Value);

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!string.IsNullOrEmpty(returnUrl))
        {
            if (_allowedCallbackUrls.Contains(returnUrl))
                return Redirect($"{returnUrl}?token={Uri.EscapeDataString(token)}");

            _logger.LogWarning("Rejected non-whitelisted returnUrl: {ReturnUrl}", returnUrl);
        }

        return Redirect($"{webBase}/login#token={Uri.EscapeDataString(token)}");
    }

    [HttpPost("logout")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    // Nettoie le cookie OAuth avant de rediriger vers la page d'erreur,
    // pour éviter qu'un cookie invalide/partiellement déchiffré
    // bloque toute reconnexion ultérieure.
    private async Task<IActionResult> RedirectWithErrorAsync(string webBase, string errorCode)
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect($"{webBase}/login#auth-error={errorCode}");
    }

    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult Me()
    {
        var sub = User.FindFirst("sub")?.Value;
        var name = User.FindFirst("name")?.Value;
        var login = User.FindFirst("login")?.Value;
        return Ok(new { sub, name, login });
    }

}
