using Kairu.Application.Common;
using Kairu.Application.Identity.Commands.GenerateMcpToken;
using Kairu.Application.Identity.Commands.RevokeMcpToken;
using Kairu.Application.Identity.Queries.GetMcpTokenStatus;
using Kairu.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Monbsoft.BrilliantMediator.Abstractions;

namespace Kairu.Api.McpToken;

[ApiController]
[Route("api/mcp-token")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class McpTokenController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public McpTokenController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Generates a new personal MCP token for the authenticated user.
    /// If a token already exists, it is revoked first.
    /// The raw token is returned exactly once and cannot be retrieved later.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Generate(CancellationToken ct)
    {
        var userId = _currentUserService.CurrentUserId;
        var command = new GenerateMcpTokenCommand(userId);
        var result = await _mediator.DispatchAsync<GenerateMcpTokenCommand, Kairu.Domain.Common.Result<GenerateMcpTokenResult>>(command, ct);

        if (result.IsFailure)
            return StatusCode(500, new { error = result.Error });

        return Ok(new
        {
            rawToken = result.Value.RawToken.Value,
            expiresAt = result.Value.ExpiresAt,
        });
    }

    /// <summary>
    /// Revokes the active personal MCP token for the authenticated user.
    /// Returns 404 if no token exists.
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> Revoke(CancellationToken ct)
    {
        var userId = _currentUserService.CurrentUserId;
        var command = new RevokeMcpTokenCommand(userId);
        var result = await _mediator.DispatchAsync<RevokeMcpTokenCommand, Kairu.Domain.Common.Result<RevokeMcpTokenResult>>(command, ct);

        return result.IsSuccess
            ? NoContent()
            : NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Returns the status of the authenticated user's MCP token.
    /// Never returns the raw token or hash.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetStatus(CancellationToken ct)
    {
        var userId = _currentUserService.CurrentUserId;
        var query = new GetMcpTokenStatusQuery(userId);
        var result = await _mediator.SendAsync<GetMcpTokenStatusQuery, Kairu.Domain.Common.Result<GetMcpTokenStatusResult>>(query, ct);

        if (result.IsFailure)
            return StatusCode(500, new { error = result.Error });

        return Ok(new
        {
            hasToken = result.Value.HasToken,
            expiresAt = result.Value.ExpiresAt,
        });
    }
}
