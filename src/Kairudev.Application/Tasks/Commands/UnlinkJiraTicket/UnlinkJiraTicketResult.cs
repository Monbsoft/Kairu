namespace Kairudev.Application.Tasks.Commands.UnlinkJiraTicket;

public sealed record UnlinkJiraTicketResult
{
    public bool IsSuccess { get; init; }
    public bool IsNotFound { get; init; }

    private UnlinkJiraTicketResult() { }

    public static UnlinkJiraTicketResult Success() => new() { IsSuccess = true };
    public static UnlinkJiraTicketResult NotFound() => new() { IsNotFound = true };
}
