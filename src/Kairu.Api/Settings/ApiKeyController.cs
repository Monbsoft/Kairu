using Kairu.Application.Settings.Commands.GenerateApiKey;
using Kairu.Application.Settings.Commands.RevokeApiKey;
using Kairu.Application.Settings.Queries.GetApiKey;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monbsoft.BrilliantMediator.Abstractions;

namespace Kairu.Api.Settings;

[ApiController]
[Route("api/settings/api-key")]
[Authorize]
public sealed class ApiKeyController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApiKeyController(IMediator mediator) => _mediator = mediator;

    /// <summary>Retourne le statut de la clé API (jamais le token brut).</summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await _mediator.SendAsync<GetApiKeyQuery, GetApiKeyResult>(
            new GetApiKeyQuery(), ct);
        return Ok(new { result.Exists, result.CreatedAt });
    }

    /// <summary>Génère (ou régénère) une API Key. Le token est retourné une seule fois.</summary>
    [HttpPost]
    public async Task<IActionResult> Generate(CancellationToken ct)
    {
        var result = await _mediator.DispatchAsync<GenerateApiKeyCommand, GenerateApiKeyResult>(
            new GenerateApiKeyCommand(), ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return StatusCode(StatusCodes.Status201Created, new { token = result.Token });
    }

    /// <summary>Révoque la clé API de l'utilisateur courant.</summary>
    [HttpDelete]
    public async Task<IActionResult> Revoke(CancellationToken ct)
    {
        var result = await _mediator.DispatchAsync<RevokeApiKeyCommand, RevokeApiKeyResult>(
            new RevokeApiKeyCommand(), ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return NoContent();
    }
}
