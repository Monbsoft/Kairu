namespace Kairudev.Application.Tasks.Commands.LinkJiraTicket;

public sealed record LinkJiraTicketResult
{
    public bool IsSuccess { get; init; }
    public bool IsNotFound { get; init; }
    public string? Error { get; init; }

    private LinkJiraTicketResult() { }

    public static LinkJiraTicketResult Success() => new() { IsSuccess = true };
    public static LinkJiraTicketResult NotFound() => new() { IsNotFound = true };
    public static LinkJiraTicketResult Failure(string error) => new() { Error = error };
}
