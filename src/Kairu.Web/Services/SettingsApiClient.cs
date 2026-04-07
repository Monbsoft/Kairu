using System.Net.Http.Json;

namespace Kairu.Web.Services;

public sealed record UserSettingsDto(
    string ThemePreference,
    string RingtonePreference,
    string? JiraBaseUrl,
    string? JiraEmail,
    bool JiraConfigured);

public sealed record ApiKeyStatusDto(bool Exists, DateTime? CreatedAt);
public sealed record ApiKeyTokenDto(string Token);

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

    public async Task<ApiKeyStatusDto?> GetApiKeyStatusAsync()
    {
        return await _httpClient.GetFromJsonAsync<ApiKeyStatusDto>("api/settings/api-key");
    }

    public async Task<ApiKeyTokenDto?> GenerateApiKeyAsync()
    {
        var response = await _httpClient.PostAsync("api/settings/api-key", null);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiKeyTokenDto>();
    }

    public async Task<bool> RevokeApiKeyAsync()
    {
        var response = await _httpClient.DeleteAsync("api/settings/api-key");
        return response.IsSuccessStatusCode;
    }
}
