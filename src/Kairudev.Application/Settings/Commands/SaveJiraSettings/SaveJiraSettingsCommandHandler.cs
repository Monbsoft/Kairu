using Kairudev.Domain.Settings;

namespace Kairudev.Application.Settings.Commands.SaveJiraSettings;

public sealed class SaveJiraSettingsCommandHandler
{
    private readonly IUserSettingsRepository _repository;

    public SaveJiraSettingsCommandHandler(IUserSettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<SaveJiraSettingsResult> Handle(SaveJiraSettingsCommand command)
    {
        var settings = await _repository.GetAsync();
        settings.UpdateJiraSettings(command.JiraBaseUrl, command.JiraEmail, command.JiraApiToken);
        await _repository.UpdateAsync(settings);
        return SaveJiraSettingsResult.Success();
    }
}
