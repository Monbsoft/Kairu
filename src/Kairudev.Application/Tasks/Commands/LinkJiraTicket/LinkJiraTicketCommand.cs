namespace Kairudev.Application.Tasks.Commands.LinkJiraTicket;

public sealed record LinkJiraTicketCommand(Guid TaskId, string JiraTicketKey);
