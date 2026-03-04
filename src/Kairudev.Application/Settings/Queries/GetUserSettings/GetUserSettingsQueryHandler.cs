using Kairudev.Application.Common;
using Kairudev.Application.Settings.Common;
using Kairudev.Domain.Settings;

namespace Kairudev.Application.Settings.Queries.GetUserSettings;

public sealed class GetUserSettingsQueryHandler
{
    private readonly IUserSettingsRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public GetUserSettingsQueryHandler(IUserSettingsRepository repository, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<GetUserSettingsResult> Handle(GetUserSettingsQuery query)
    {
        var userId = _currentUserService.CurrentUserId;
        var settings = await _repository.GetByUserIdAsync(userId);

        var jiraConfigured = !string.IsNullOrWhiteSpace(settings.JiraBaseUrl)
            && !string.IsNullOrWhiteSpace(settings.JiraEmail)
            && !string.IsNullOrWhiteSpace(settings.JiraApiToken);

        var viewModel = new UserSettingsViewModel(
            ThemePreference: settings.ThemePreference.ToString(),
            RingtonePreference: settings.RingtonePreference.ToString(),
            JiraBaseUrl: settings.JiraBaseUrl,
            JiraEmail: settings.JiraEmail,
            JiraConfigured: jiraConfigured
        );

        return GetUserSettingsResult.Success(viewModel);
    }
}
