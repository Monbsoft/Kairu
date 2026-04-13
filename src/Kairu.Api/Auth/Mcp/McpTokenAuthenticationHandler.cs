using System.Security.Claims;
using System.Text.Encodings.Web;
using Kairu.Application.Identity.Queries.ValidateMcpToken;
using Kairu.Domain.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Monbsoft.BrilliantMediator.Abstractions;

namespace Kairu.Api.Auth.Mcp;

/// <summary>
/// Authentication handler for MCP personal tokens.
/// Reads the raw token from the Authorization: Bearer header,
/// dispatches ValidateMcpTokenQuery to validate it, and builds
/// a ClaimsPrincipal with the associated UserId as "sub" claim.
/// </summary>
public sealed class McpTokenAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IMediator _mediator;

    public McpTokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IMediator mediator)
        : base(options, logger, encoder)
    {
        _mediator = mediator;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var rawTokenValue = authHeader["Bearer ".Length..].Trim();
        if (string.IsNullOrEmpty(rawTokenValue))
            return AuthenticateResult.Fail("Missing token value.");

        var rawToken = new McpRawToken(rawTokenValue);
        var query = new ValidateMcpTokenQuery(rawToken);

        var result = await _mediator.SendAsync<ValidateMcpTokenQuery, Kairu.Domain.Common.Result<ValidateMcpTokenResult>>(query, Context.RequestAborted);

        if (result.IsFailure)
        {
            Logger.LogWarning("MCP token authentication failed: {Error}", result.Error);
            return AuthenticateResult.Fail("Invalid or expired MCP token.");
        }

        var userId = result.Value.UserId;

        var claims = new[]
        {
            new Claim("sub", userId.Value.ToString()),
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
