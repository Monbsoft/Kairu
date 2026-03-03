namespace Kairudev.Application.Tickets.Queries.GetAssignedJiraTickets;

public sealed record GetAssignedJiraTicketsResult
{
    public bool IsSuccess { get; init; }
    public bool IsNotConfigured { get; init; }
    public IReadOnlyList<JiraTicketDto>? Tickets { get; init; }
    public string? Error { get; init; }

    private GetAssignedJiraTicketsResult() { }

    public static GetAssignedJiraTicketsResult Success(IReadOnlyList<JiraTicketDto> tickets) =>
        new() { IsSuccess = true, Tickets = tickets };

    public static GetAssignedJiraTicketsResult NotConfigured() =>
        new() { IsNotConfigured = true, Error = "Jira is not configured." };

    public static GetAssignedJiraTicketsResult Failure(string error) =>
        new() { Error = error };
}
