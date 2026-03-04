using Kairudev.Application.Settings.Commands.SaveJiraSettings;
using Kairudev.Application.Settings.Commands.SaveRingtonePreference;
using Kairudev.Application.Settings.Commands.SaveThemePreference;
using Kairudev.Application.Settings.Queries.GetUserSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kairudev.Api.Settings;

[ApiController]
[Route("api/settings")]
[Authorize]
public sealed class SettingsController : ControllerBase
{
    private readonly GetUserSettingsQueryHandler _getUserSettings;
    private readonly SaveThemePreferenceCommandHandler _saveThemePreference;
    private readonly SaveRingtonePreferenceCommandHandler _saveRingtonePreference;
    private readonly SaveJiraSettingsCommandHandler _saveJiraSettings;

    public SettingsController(
        GetUserSettingsQueryHandler getUserSettings,
        SaveThemePreferenceCommandHandler saveThemePreference,
        SaveRingtonePreferenceCommandHandler saveRingtonePreference,
        SaveJiraSettingsCommandHandler saveJiraSettings)
    {
        _getUserSettings = getUserSettings;
        _saveThemePreference = saveThemePreference;
        _saveRingtonePreference = saveRingtonePreference;
        _saveJiraSettings = saveJiraSettings;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var query = new GetUserSettingsQuery();
        var result = await _getUserSettings.Handle(query);
        return Ok(result.Settings);
    }

    [HttpPut("theme")]
    public async Task<IActionResult> SaveThemePreference([FromBody] SaveThemePreferenceRequest request)
    {
        var command = new SaveThemePreferenceCommand(request.ThemePreference);
        var result = await _saveThemePreference.Handle(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }

    [HttpPut("ringtone")]
    public async Task<IActionResult> SaveRingtonePreference([FromBody] SaveRingtonePreferenceRequest request)
    {
        var command = new SaveRingtonePreferenceCommand(request.RingtonePreference);
        var result = await _saveRingtonePreference.Handle(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }

    [HttpPut("jira")]
    public async Task<IActionResult> SaveJiraSettings([FromBody] SaveJiraSettingsRequest request)
    {
        var command = new SaveJiraSettingsCommand(request.JiraBaseUrl, request.JiraEmail, request.JiraApiToken);
        var result = await _saveJiraSettings.Handle(command);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }
}

public sealed record SaveThemePreferenceRequest(string ThemePreference);
public sealed record SaveRingtonePreferenceRequest(string RingtonePreference);
public sealed record SaveJiraSettingsRequest(string? JiraBaseUrl, string? JiraEmail, string? JiraApiToken);
