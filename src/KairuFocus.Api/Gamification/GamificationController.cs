using KairuFocus.Application.Gamification.Queries.GetXpSummary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monbsoft.BrilliantMediator.Abstractions;

namespace KairuFocus.Api.Gamification;

[ApiController]
[Route("api/gamification")]
[Authorize]
public sealed class GamificationController : ControllerBase
{
    private readonly IMediator _mediator;

    public GamificationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("xp")]
    public async Task<IActionResult> GetXp(CancellationToken ct)
    {
        var result = await _mediator.SendAsync<GetXpSummaryQuery, GetXpSummaryResult>(new GetXpSummaryQuery(), ct);
        return Ok(result);
    }
}
