using KairuFocus.Application.Pomodoro.Commands.CompleteSession;
using KairuFocus.Application.Pomodoro.Commands.CreateTaskDuringSession;
using KairuFocus.Application.Pomodoro.Commands.InterruptSession;
using KairuFocus.Application.Pomodoro.Commands.LinkTask;
using KairuFocus.Application.Pomodoro.Commands.UnlinkTask;
using KairuFocus.Application.Pomodoro.Commands.SaveSettings;
using KairuFocus.Application.Pomodoro.Commands.StartSession;
using KairuFocus.Application.Pomodoro.Commands.UpdateTaskStatus;
using KairuFocus.Application.Pomodoro.Queries.GetCurrentSession;
using KairuFocus.Application.Pomodoro.Queries.GetSettings;
using KairuFocus.Application.Pomodoro.Queries.GetSuggestedSessionType;
using KairuFocus.Application.Pomodoro.Queries.GetTodaySprintSessions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monbsoft.BrilliantMediator.Abstractions;
using PomodoroErrors = KairuFocus.Domain.Pomodoro.DomainErrors;

namespace KairuFocus.Api.Pomodoro;

[ApiController]
[Route("api/pomodoro")]
[Authorize]
public sealed class PomodoroController : ControllerBase
{
    private readonly IMediator _mediator;

    public PomodoroController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ── Settings ───────────────────────────────────────────────────────────

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings(CancellationToken ct)
    {
        var result = await _mediator.SendAsync<GetSettingsQuery, GetSettingsResult>(new GetSettingsQuery(), ct);
        return Ok(result.Settings);
    }

    [HttpPut("settings")]
    public async Task<IActionResult> SaveSettings([FromBody] SaveSettingsCommand command, CancellationToken ct)
    {
        var result = await _mediator.DispatchAsync<SaveSettingsCommand, SaveSettingsResult>(command, ct);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(new { error = result.ValidationError });
    }

    // ── Session ────────────────────────────────────────────────────────────

    [HttpGet("session/suggested")]
    public async Task<IActionResult> GetSuggestedSessionType(CancellationToken ct)
    {
        var result = await _mediator.SendAsync<GetSuggestedSessionTypeQuery, GetSuggestedSessionTypeResult>(new GetSuggestedSessionTypeQuery(), ct);
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
        var result = await _mediator.SendAsync<GetCurrentSessionQuery, GetCurrentSessionResult>(new GetCurrentSessionQuery(), ct);
        return result.HasSession
            ? Ok(result.Session)
            : NoContent();
    }

    [HttpPost("session")]
    public async Task<IActionResult> StartSession([FromQuery] string? type, CancellationToken ct)
    {
        var result = await _mediator.DispatchAsync<StartSessionCommand, StartSessionResult>(new StartSessionCommand(type), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetCurrentSession), null, result.Session)
            : Conflict(new { error = result.Error });
    }

    [HttpPost("session/free-sprint")]
    public async Task<IActionResult> StartFreeSprint([FromBody] StartFreeSprintBody body, CancellationToken ct)
    {
        var command = new StartSessionCommand("Sprint", IsFreeSession: true, JournalComment: body.JournalComment);
        var result = await _mediator.DispatchAsync<StartSessionCommand, StartSessionResult>(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetCurrentSession), null, result.Session)
            : Conflict(new { error = result.Error });
    }

    [HttpGet("sessions/today-sprints")]
    public async Task<IActionResult> GetTodaySprintSessions(CancellationToken ct)
    {
        var result = await _mediator.SendAsync<GetTodaySprintSessionsQuery, GetTodaySprintSessionsResult>(
            new GetTodaySprintSessionsQuery(), ct);
        return Ok(result.Sessions);
    }

    [HttpPatch("session/complete")]
    public async Task<IActionResult> CompleteSession(CancellationToken ct)
    {
        var result = await _mediator.DispatchAsync<CompleteSessionCommand, CompleteSessionResult>(new CompleteSessionCommand(), ct);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(new { error = result.Error });
    }

    [HttpPatch("session/interrupt")]
    public async Task<IActionResult> InterruptSession(CancellationToken ct)
    {
        var result = await _mediator.DispatchAsync<InterruptSessionCommand, InterruptSessionResult>(new InterruptSessionCommand(), ct);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(new { error = result.Error });
    }

    // ── Tasks within session ───────────────────────────────────────────────

    [HttpPost("session/tasks/{id:guid}")]
    public async Task<IActionResult> LinkTask(Guid id, CancellationToken ct)
    {
        var result = await _mediator.DispatchAsync<LinkTaskCommand, LinkTaskResult>(new LinkTaskCommand(id), ct);

        return result switch
        {
            { IsSuccess: true } => NoContent(),
            { IsNotFound: true } => NotFound(),
            _ => BadRequest(new { error = result.Error })
        };
    }

    [HttpDelete("session/tasks/{id:guid}")]
    public async Task<IActionResult> UnlinkTask(Guid id, CancellationToken ct)
    {
        var result = await _mediator.DispatchAsync<UnlinkTaskCommand, UnlinkTaskResult>(new UnlinkTaskCommand(id), ct);

        return result switch
        {
            { IsSuccess: true } => NoContent(),
            { IsNotFound: true } => NotFound(),
            _ when result.Error == PomodoroErrors.Pomodoro.NoActiveSession => Conflict(new { error = result.Error }),
            _ => BadRequest(new { error = result.Error })
        };
    }

    [HttpPost("session/tasks")]
    public async Task<IActionResult> CreateTaskDuringSession(
        [FromBody] CreateTaskDuringSessionCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.DispatchAsync<CreateTaskDuringSessionCommand, CreateTaskDuringSessionResult>(command, ct);
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
        var result = await _mediator.DispatchAsync<UpdateTaskStatusCommand, UpdateTaskStatusResult>(
            new UpdateTaskStatusCommand(id, body.TargetStatus), ct);

        return result switch
        {
            { IsSuccess: true } => Ok(result.Task),
            { IsNotFound: true } => NotFound(),
            _ => BadRequest(new { error = result.Error })
        };
    }
}

public sealed record StartFreeSprintBody(string? JournalComment);
public sealed record UpdateTaskStatusBody(string TargetStatus);
