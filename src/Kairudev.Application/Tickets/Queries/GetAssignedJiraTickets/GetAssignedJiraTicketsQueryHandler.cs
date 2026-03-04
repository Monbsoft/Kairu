using Kairudev.Application.Common;
using Kairudev.Domain.Settings;

namespace Kairudev.Application.Tickets.Queries.GetAssignedJiraTickets;

public sealed class GetAssignedJiraTicketsQueryHandler
{
    private readonly IUserSettingsRepository _settingsRepository;
    private readonly IJiraTicketService _jiraService;
    private readonly ICurrentUserService _currentUserService;

    public GetAssignedJiraTicketsQueryHandler(
        IUserSettingsRepository settingsRepository,
        IJiraTicketService jiraService,
        ICurrentUserService currentUserService)
    {
        _settingsRepository = settingsRepository;
        _jiraService = jiraService;
        _currentUserService = currentUserService;
    }

    public async Task<GetAssignedJiraTicketsResult> HandleAsync(
        GetAssignedJiraTicketsQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;
        var settings = await _settingsRepository.GetByUserIdAsync(userId);

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
