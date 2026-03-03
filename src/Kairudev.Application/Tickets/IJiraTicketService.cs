using Kairudev.Domain.Common;

namespace Kairudev.Application.Tickets;

public sealed record JiraTicketDto(
    string Key,
    string Summary,
    string Status,
    string? Priority);

public interface IJiraTicketService
{
    Task<Result<IReadOnlyList<JiraTicketDto>>> GetAssignedTicketsAsync(
        string baseUrl, string email, string apiToken,
        CancellationToken cancellationToken = default);
}
