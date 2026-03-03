using Kairudev.Domain.Settings;

namespace Kairudev.Application.Tickets.Queries.GetAssignedJiraTickets;

public sealed class GetAssignedJiraTicketsQueryHandler
{
    private readonly IUserSettingsRepository _settingsRepository;
    private readonly IJiraTicketService _jiraService;

    public GetAssignedJiraTicketsQueryHandler(
        IUserSettingsRepository settingsRepository,
        IJiraTicketService jiraService)
    {
        _settingsRepository = settingsRepository;
        _jiraService = jiraService;
    }

    public async Task<GetAssignedJiraTicketsResult> HandleAsync(
        GetAssignedJiraTicketsQuery query,
        CancellationToken cancellationToken = default)
    {
        var settings = await _settingsRepository.GetAsync();

        if (string.IsNullOrWhiteSpace(settings.JiraBaseUrl)
            || string.IsNullOrWhiteSpace(settings.JiraEmail)
            || string.IsNullOrWhiteSpace(settings.JiraApiToken))
            return GetAssignedJiraTicketsResult.NotConfigured();

        var result = await _jiraService.GetAssignedTicketsAsync(
            settings.JiraBaseUrl, settings.JiraEmail, settings.JiraApiToken,
            cancellationToken);

        return result.IsSuccess
            ? GetAssignedJiraTicketsResult.Success(result.Value)
            : GetAssignedJiraTicketsResult.Failure(result.Error);
    }
}
