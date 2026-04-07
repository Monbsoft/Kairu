using System.ComponentModel;
using System.Text.Json;
using Kairu.Application.Tasks.Commands.AddTask;
using Kairu.Application.Tasks.Commands.CompleteTask;
using Kairu.Application.Tasks.Commands.DeleteTask;
using Kairu.Application.Tasks.Queries.ListTasks;
using ModelContextProtocol.Server;
using Monbsoft.BrilliantMediator.Abstractions;

namespace Kairu.Api.Mcp;

[McpServerToolType]
public sealed class KairuMcpTools(IMediator mediator)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [McpServerTool, Description("Create a new task in Kairu. Returns the created task as JSON.")]
    public async Task<string> create_task(
        [Description("Task title (required, max 200 characters)")] string title,
        [Description("Optional task description")] string? description = null)
    {
        var result = await mediator.DispatchAsync<AddTaskCommand, AddTaskResult>(
            new AddTaskCommand(title, description));

        return result.IsSuccess
            ? JsonSerializer.Serialize(result.Task, JsonOptions)
            : $"Error: {result.Error}";
    }

    [McpServerTool, Description("List tasks in Kairu. Returns a JSON array of tasks.")]
    public async Task<string> list_tasks(
        [Description("Status filter: 'all', 'pending', 'inprogress', 'done', 'openonly' (default: 'openonly')")] string status = "openonly")
    {
        var filter = status.ToLowerInvariant() switch
        {
            "all" => TaskStatusFilter.All,
            "pending" => TaskStatusFilter.Pending,
            "inprogress" => TaskStatusFilter.InProgress,
            "done" => TaskStatusFilter.Done,
            _ => TaskStatusFilter.OpenOnly
        };

        var result = await mediator.SendAsync<ListTasksQuery, ListTasksResult>(
            new ListTasksQuery(StatusFilter: filter));

        return JsonSerializer.Serialize(result.Tasks, JsonOptions);
    }

    [McpServerTool, Description("Mark a task as completed in Kairu.")]
    public async Task<string> complete_task(
        [Description("Task ID (GUID format, e.g. '3fa85f64-5717-4562-b3fc-2c963f66afa6')")] string taskId)
    {
        if (!Guid.TryParse(taskId, out var guid))
            return "Error: taskId must be a valid GUID.";

        var result = await mediator.DispatchAsync<CompleteTaskCommand, CompleteTaskResult>(
            new CompleteTaskCommand(guid));

        if (result.IsSuccess) return $"Task {taskId} marked as completed.";
        if (result.IsNotFound) return $"Error: Task {taskId} not found.";
        return $"Error: {result.Error}";
    }

    [McpServerTool, Description("Delete a task from Kairu.")]
    public async Task<string> delete_task(
        [Description("Task ID (GUID format, e.g. '3fa85f64-5717-4562-b3fc-2c963f66afa6')")] string taskId)
    {
        if (!Guid.TryParse(taskId, out var guid))
            return "Error: taskId must be a valid GUID.";

        var result = await mediator.DispatchAsync<DeleteTaskCommand, DeleteTaskResult>(
            new DeleteTaskCommand(guid));

        if (result.IsSuccess) return $"Task {taskId} deleted.";
        if (result.IsNotFound) return $"Error: Task {taskId} not found.";
        return $"Error: {result.Error}";
    }
}
