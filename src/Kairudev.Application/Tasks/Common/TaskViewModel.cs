using Kairudev.Domain.Tasks;

namespace Kairudev.Application.Tasks.Common;

public sealed record TaskViewModel(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    string? JiraTicketKey)
{
    public static TaskViewModel From(DeveloperTask task) =>
        new(
            task.Id.Value,
            task.Title.Value,
            task.Description?.Value,
            task.Status.ToString(),
            task.CreatedAt,
            task.CompletedAt,
            task.JiraTicketKey?.Value);
}
