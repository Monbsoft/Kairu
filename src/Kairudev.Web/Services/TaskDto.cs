namespace Kairudev.Web.Services;

public sealed record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    string? JiraTicketKey);
