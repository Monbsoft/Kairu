using Kairudev.Application.Tasks.Commands.AddTask;
using Kairudev.Application.Tasks.Commands.ChangeTaskStatus;
using Kairudev.Application.Tasks.Commands.CompleteTask;
using Kairudev.Application.Tasks.Commands.DeleteTask;
using Kairudev.Application.Tasks.Commands.LinkJiraTicket;
using Kairudev.Application.Tasks.Commands.UnlinkJiraTicket;
using Kairudev.Application.Tasks.Commands.UpdateTask;
using Kairudev.Application.Tasks.Queries.ListTasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kairudev.Api.Tasks;

[ApiController]
[Route("api/tasks")]
[Authorize]
public sealed class TasksController : ControllerBase
{
    private readonly AddTaskCommandHandler _addTask;
    private readonly ListTasksQueryHandler _listTasks;
    private readonly CompleteTaskCommandHandler _completeTask;
    private readonly DeleteTaskCommandHandler _deleteTask;
    private readonly ChangeTaskStatusCommandHandler _changeStatus;
    private readonly UpdateTaskCommandHandler _updateTask;
    private readonly LinkJiraTicketCommandHandler _linkJiraTicket;
    private readonly UnlinkJiraTicketCommandHandler _unlinkJiraTicket;

    public TasksController(
        AddTaskCommandHandler addTask,
        ListTasksQueryHandler listTasks,
        CompleteTaskCommandHandler completeTask,
        DeleteTaskCommandHandler deleteTask,
        ChangeTaskStatusCommandHandler changeStatus,
        UpdateTaskCommandHandler updateTask,
        LinkJiraTicketCommandHandler linkJiraTicket,
        UnlinkJiraTicketCommandHandler unlinkJiraTicket)
    {
        _addTask = addTask;
        _listTasks = listTasks;
        _completeTask = completeTask;
        _deleteTask = deleteTask;
        _changeStatus = changeStatus;
        _updateTask = updateTask;
        _linkJiraTicket = linkJiraTicket;
        _unlinkJiraTicket = unlinkJiraTicket;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _listTasks.HandleAsync(new ListTasksQuery(), ct);
        return Ok(result.Tasks);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AddTaskCommand command, CancellationToken ct)
    {
        var result = await _addTask.HandleAsync(command, ct);

        return result.IsSuccess
            ? Created($"api/tasks/{result.Task!.Id}", result.Task)
            : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken ct)
    {
        var result = await _completeTask.HandleAsync(new CompleteTaskCommand(id), ct);

        return result switch
        {
            { IsSuccess: true } => NoContent(),
            { IsNotFound: true } => NotFound(),
            _ => BadRequest(new { error = result.Error })
        };
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _deleteTask.HandleAsync(new DeleteTaskCommand(id), ct);

        return result switch
        {
            { IsSuccess: true } => NoContent(),
            { IsNotFound: true } => NotFound(),
            _ => StatusCode(500, new { error = result.Error })
        };
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        [FromBody] ChangeTaskStatusBody body,
        CancellationToken ct)
    {
        var result = await _changeStatus.HandleAsync(
            new ChangeTaskStatusCommand(id, body.NewStatus), ct);

        return result switch
        {
            { IsSuccess: true } => Ok(result.Task),
            { IsNotFound: true } => NotFound(),
            { ValidationError: not null } => BadRequest(new { error = result.ValidationError }),
            { ConflictError: not null } => Conflict(new { error = result.ConflictError }),
            _ => StatusCode(500)
        };
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTaskBody body,
        CancellationToken ct)
    {
        var result = await _updateTask.HandleAsync(
            new UpdateTaskCommand(id, body.Title, body.Description), ct);

        return result switch
        {
            { IsSuccess: true } => Ok(result.Task),
            { IsNotFound: true } => NotFound(),
            { Error: not null } => BadRequest(new { error = result.Error }),
            _ => StatusCode(500)
        };
    }

    [HttpPut("{id:guid}/jira-ticket")]
    public async Task<IActionResult> LinkJiraTicket(
        Guid id,
        [FromBody] LinkJiraTicketBody body,
        CancellationToken ct)
    {
        var result = await _linkJiraTicket.HandleAsync(
            new LinkJiraTicketCommand(id, body.JiraTicketKey), ct);

        return result switch
        {
            { IsSuccess: true } => NoContent(),
            { IsNotFound: true } => NotFound(),
            _ => BadRequest(new { error = result.Error })
        };
    }

    [HttpDelete("{id:guid}/jira-ticket")]
    public async Task<IActionResult> UnlinkJiraTicket(Guid id, CancellationToken ct)
    {
        var result = await _unlinkJiraTicket.HandleAsync(
            new UnlinkJiraTicketCommand(id), ct);

        return result switch
        {
            { IsSuccess: true } => NoContent(),
            { IsNotFound: true } => NotFound(),
            _ => StatusCode(500)
        };
    }
}

public sealed record ChangeTaskStatusBody(string NewStatus);
public sealed record UpdateTaskBody(string Title, string? Description);
public sealed record LinkJiraTicketBody(string JiraTicketKey);
