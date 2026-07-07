using System.Net.Http.Json;

namespace KairuFocus.Web.Services;

public sealed record UserSettingsDto(
    string ThemePreference,
    string RingtonePreference,
    int AlarmVolume,
    string? JiraBaseUrl,
    string? JiraEmail,
    bool JiraConfigured);

public sealed class SettingsApiClient
{
    private readonly HttpClient _httpClient;

    public SettingsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UserSettingsDto?> GetSettingsAsync()
    {
        return await _httpClient.GetFromJsonAsync<UserSettingsDto>("api/settings");
    }

    public async Task<bool> SaveThemePreferenceAsync(string themePreference)
    {
        var request = new { ThemePreference = themePreference };
        var response = await _httpClient.PutAsJsonAsync("api/settings/theme", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SaveRingtonePreferenceAsync(string ringtonePreference)
    {
        var request = new { RingtonePreference = ringtonePreference };
        var response = await _httpClient.PutAsJsonAsync("api/settings/ringtone", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SaveAlarmVolumeAsync(int volume)
    {
        var request = new { Volume = volume };
        var response = await _httpClient.PutAsJsonAsync("api/settings/volume", request);
        return response.IsSuccessStatusCode;
    }

}
