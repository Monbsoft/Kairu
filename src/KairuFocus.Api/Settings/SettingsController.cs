using KairuFocus.Application.Settings.Commands.SaveAlarmVolume;
using KairuFocus.Application.Settings.Commands.SaveRingtonePreference;
using KairuFocus.Application.Settings.Commands.SaveThemePreference;
using KairuFocus.Application.Settings.Queries.GetUserSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monbsoft.BrilliantMediator.Abstractions;

namespace KairuFocus.Api.Settings;

[ApiController]
[Route("api/settings")]
[Authorize]
public sealed class SettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await _mediator.SendAsync<GetUserSettingsQuery, GetUserSettingsResult>(new GetUserSettingsQuery(), ct);
        return Ok(result.Settings);
    }

    [HttpPut("theme")]
    public async Task<IActionResult> SaveThemePreference([FromBody] SaveThemePreferenceRequest request, CancellationToken ct)
    {
        var command = new SaveThemePreferenceCommand(request.ThemePreference);
        var result = await _mediator.DispatchAsync<SaveThemePreferenceCommand, SaveThemePreferenceResult>(command, ct);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }

    [HttpPut("ringtone")]
    public async Task<IActionResult> SaveRingtonePreference([FromBody] SaveRingtonePreferenceRequest request, CancellationToken ct)
    {
        var command = new SaveRingtonePreferenceCommand(request.RingtonePreference);
        var result = await _mediator.DispatchAsync<SaveRingtonePreferenceCommand, SaveRingtonePreferenceResult>(command, ct);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }

    [HttpPut("volume")]
    public async Task<IActionResult> SaveAlarmVolume([FromBody] SaveAlarmVolumeRequest request, CancellationToken ct)
    {
        var command = new SaveAlarmVolumeCommand(request.Volume);
        var result = await _mediator.DispatchAsync<SaveAlarmVolumeCommand, SaveAlarmVolumeResult>(command, ct);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }
}

public sealed record SaveThemePreferenceRequest(string ThemePreference);
public sealed record SaveRingtonePreferenceRequest(string RingtonePreference);
public sealed record SaveAlarmVolumeRequest(int Volume);
