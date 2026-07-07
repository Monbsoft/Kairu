using KairuFocus.Application.Common;
using KairuFocus.Application.Settings.Common;
using KairuFocus.Domain.Settings;
using Microsoft.Extensions.Logging;
using Monbsoft.BrilliantMediator.Abstractions.Queries;

namespace KairuFocus.Application.Settings.Queries.GetUserSettings;

public sealed class GetUserSettingsQueryHandler : IQueryHandler<GetUserSettingsQuery, GetUserSettingsResult>
{
    private readonly IUserSettingsRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetUserSettingsQueryHandler> _logger;

    public GetUserSettingsQueryHandler(
        IUserSettingsRepository repository,
        ICurrentUserService currentUserService,
        ILogger<GetUserSettingsQueryHandler> logger)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<GetUserSettingsResult> Handle(GetUserSettingsQuery query, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;
        _logger.LogDebug("Fetching user settings for user {UserId}", userId);
        var settings = await _repository.GetByUserIdAsync(userId);

        var jiraConfigured = !string.IsNullOrWhiteSpace(settings.JiraBaseUrl)
            && !string.IsNullOrWhiteSpace(settings.JiraEmail)
            && !string.IsNullOrWhiteSpace(settings.JiraApiToken);

        var viewModel = new UserSettingsViewModel(
            ThemePreference: settings.ThemePreference.ToString(),
            RingtonePreference: settings.RingtonePreference.ToString(),
            AlarmVolume: settings.AlarmVolume,
            JiraBaseUrl: settings.JiraBaseUrl,
            JiraEmail: settings.JiraEmail,
            JiraConfigured: jiraConfigured
        );

        return GetUserSettingsResult.Success(viewModel);
    }
}
