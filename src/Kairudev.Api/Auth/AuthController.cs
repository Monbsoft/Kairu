using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Kairudev.Application.Identity.Commands.GetOrCreateUser;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Kairudev.Api.Auth;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly GetOrCreateUserCommandHandler _handler;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    private readonly IReadOnlySet<string> _allowedCallbackUrls;

    public AuthController(
        GetOrCreateUserCommandHandler handler,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _handler = handler;
        _configuration = configuration;
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
            return Redirect($"{webBase}/login#auth-error=denied");
        }

        var githubId = result.Principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var login = result.Principal.FindFirst("urn:github:login")?.Value
            ?? result.Principal.FindFirst(ClaimTypes.Name)?.Value ?? "unknown";
        var displayName = result.Principal.FindFirst(ClaimTypes.Name)?.Value ?? login;
        var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(githubId))
            return Redirect($"{webBase}/login#auth-error=no-id");

        var command = new GetOrCreateUserCommand(githubId, login, displayName, email);
        var userResult = await _handler.Handle(command, ct);

        if (userResult.IsFailure)
        {
            _logger.LogError("Failed to get or create user: {Error}", userResult.Error);
            return Redirect($"{webBase}/login#auth-error=server");
        }

        var token = GenerateJwt(userResult.Value);

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!string.IsNullOrEmpty(returnUrl))
        {
            if (_allowedCallbackUrls.Contains(returnUrl))
                return Redirect($"{returnUrl}?token={Uri.EscapeDataString(token)}");

            _logger.LogWarning("Rejected non-whitelisted returnUrl: {ReturnUrl}", returnUrl);
        }

        return Redirect($"{webBase}/login#token={Uri.EscapeDataString(token)}");
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

    private string GenerateJwt(GetOrCreateUserResult user)
    {
        var secretKey = _configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiryHours = _configuration.GetValue<int>("Jwt:ExpiryHours", 24);

        var claims = new[]
        {
            new Claim("sub", user.UserId.Value),
            new Claim("name", user.DisplayName),
            new Claim("login", user.Login),
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiryHours),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
