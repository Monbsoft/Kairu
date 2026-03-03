using System.Net.Http.Json;

namespace Kairudev.Maui.Services;

public sealed record UserSettingsDto(
    string ThemePreference,
    string RingtonePreference,
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
}
