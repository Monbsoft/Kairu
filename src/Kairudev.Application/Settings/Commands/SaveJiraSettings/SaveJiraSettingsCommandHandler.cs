using Kairudev.Application.Common;
using Kairudev.Domain.Settings;

namespace Kairudev.Application.Settings.Commands.SaveJiraSettings;

public sealed class SaveJiraSettingsCommandHandler
{
    private readonly IUserSettingsRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public SaveJiraSettingsCommandHandler(IUserSettingsRepository repository, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<SaveJiraSettingsResult> Handle(SaveJiraSettingsCommand command)
    {
        var userId = _currentUserService.CurrentUserId;
        var settings = await _repository.GetByUserIdAsync(userId);
        settings.UpdateJiraSettings(command.JiraBaseUrl, command.JiraEmail, command.JiraApiToken);
        await _repository.UpdateAsync(settings);
        return SaveJiraSettingsResult.Success();
    }
}
