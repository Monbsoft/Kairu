using Kairudev.Application.Pomodoro.Commands.CompleteSession;
using Kairudev.Application.Pomodoro.Commands.CreateTaskDuringSession;
using Kairudev.Application.Pomodoro.Commands.InterruptSession;
using Kairudev.Application.Pomodoro.Commands.LinkTask;
using Kairudev.Application.Pomodoro.Commands.SaveSettings;
using Kairudev.Application.Pomodoro.Commands.StartSession;
using Kairudev.Application.Pomodoro.Commands.UpdateTaskStatus;
using Kairudev.Application.Pomodoro.Queries.GetCurrentSession;
using Kairudev.Application.Pomodoro.Queries.GetSettings;
using Kairudev.Application.Pomodoro.Queries.GetSuggestedSessionType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kairudev.Api.Pomodoro;

[ApiController]
[Route("api/pomodoro")]
[Authorize]
public sealed class PomodoroController : ControllerBase
{
    private readonly GetSettingsQueryHandler _getSettings;
    private readonly SaveSettingsCommandHandler _saveSettings;
    private readonly GetSuggestedSessionTypeQueryHandler _getSuggestedType;
    private readonly GetCurrentSessionQueryHandler _getCurrentSession;
    private readonly StartSessionCommandHandler _startSession;
    private readonly CompleteSessionCommandHandler _completeSession;
    private readonly InterruptSessionCommandHandler _interruptSession;
    private readonly LinkTaskCommandHandler _linkTask;
    private readonly CreateTaskDuringSessionCommandHandler _createTask;
    private readonly UpdateTaskStatusCommandHandler _updateTaskStatus;

    public PomodoroController(
        GetSettingsQueryHandler getSettings,
        SaveSettingsCommandHandler saveSettings,
        GetSuggestedSessionTypeQueryHandler getSuggestedType,
        GetCurrentSessionQueryHandler getCurrentSession,
        StartSessionCommandHandler startSession,
        CompleteSessionCommandHandler completeSession,
        InterruptSessionCommandHandler interruptSession,
        LinkTaskCommandHandler linkTask,
        CreateTaskDuringSessionCommandHandler createTask,
        UpdateTaskStatusCommandHandler updateTaskStatus)
    {
        _getSettings = getSettings;
        _saveSettings = saveSettings;
        _getSuggestedType = getSuggestedType;
        _getCurrentSession = getCurrentSession;
        _startSession = startSession;
        _completeSession = completeSession;
        _interruptSession = interruptSession;
        _linkTask = linkTask;
        _createTask = createTask;
        _updateTaskStatus = updateTaskStatus;
    }

    // ── Settings ───────────────────────────────────────────────────────────

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings(CancellationToken ct)
    {
        var result = await _getSettings.HandleAsync(new GetSettingsQuery(), ct);
        return Ok(result.Settings);
    }

    [HttpPut("settings")]
    public async Task<IActionResult> SaveSettings([FromBody] SaveSettingsCommand command, CancellationToken ct)
    {
        var result = await _saveSettings.HandleAsync(command, ct);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(new { error = result.ValidationError });
    }

    // ── Session ────────────────────────────────────────────────────────────

    [HttpGet("session/suggested")]
    public async Task<IActionResult> GetSuggestedSessionType(CancellationToken ct)
    {
        var result = await _getSuggestedType.HandleAsync(new GetSuggestedSessionTypeQuery(), ct);
        return Ok(new
        {
            SuggestedType = result.SuggestedType.ToString(),
            SprintDurationMinutes = result.SprintDurationMinutes,
            ShortBreakDurationMinutes = result.ShortBreakDurationMinutes,
            LongBreakDurationMinutes = result.LongBreakDurationMinutes
        });
    }

    [HttpGet("session")]
    public async Task<IActionResult> GetCurrentSession(CancellationToken ct)
    {
        var result = await _getCurrentSession.HandleAsync(new GetCurrentSessionQuery(), ct);
        return result.HasSession
            ? Ok(result.Session)
            : NoContent();
    }

    [HttpPost("session")]
    public async Task<IActionResult> StartSession([FromQuery] string? type, CancellationToken ct)
    {
        var result = await _startSession.HandleAsync(new StartSessionCommand(type), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetCurrentSession), null, result.Session)
            : Conflict(new { error = result.Error });
    }

    [HttpPatch("session/complete")]
    public async Task<IActionResult> CompleteSession(CancellationToken ct)
    {
        var result = await _completeSession.HandleAsync(new CompleteSessionCommand(), ct);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(new { error = result.Error });
    }

    [HttpPatch("session/interrupt")]
    public async Task<IActionResult> InterruptSession(CancellationToken ct)
    {
        var result = await _interruptSession.HandleAsync(new InterruptSessionCommand(), ct);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(new { error = result.Error });
    }

    // ── Tasks within session ───────────────────────────────────────────────

    [HttpPost("session/tasks/{id:guid}")]
    public async Task<IActionResult> LinkTask(Guid id, CancellationToken ct)
    {
        var result = await _linkTask.HandleAsync(new LinkTaskCommand(id), ct);

        return result switch
        {
            { IsSuccess: true } => NoContent(),
            { IsNotFound: true } => NotFound(),
            _ => BadRequest(new { error = result.Error })
        };
    }

    [HttpPost("session/tasks")]
    public async Task<IActionResult> CreateTaskDuringSession(
        [FromBody] CreateTaskDuringSessionCommand command,
        CancellationToken ct)
    {
        var result = await _createTask.HandleAsync(command, ct);
        return result.IsSuccess
            ? Created($"api/tasks/{result.Task!.Id}", result.Task)
            : BadRequest(new { error = result.Error });
    }

    [HttpPatch("session/tasks/{id:guid}/status")]
    public async Task<IActionResult> UpdateTaskStatus(
        Guid id,
        [FromBody] UpdateTaskStatusBody body,
        CancellationToken ct)
    {
        var result = await _updateTaskStatus.HandleAsync(
            new UpdateTaskStatusCommand(id, body.TargetStatus), ct);

        return result switch
        {
            { IsSuccess: true } => Ok(result.Task),
            { IsNotFound: true } => NotFound(),
            _ => BadRequest(new { error = result.Error })
        };
    }
}

public sealed record UpdateTaskStatusBody(string TargetStatus);
