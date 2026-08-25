using System.Net.Http.Json;

namespace KairuFocus.Web.Services;

public sealed class GamificationApiClient
{
    private readonly HttpClient _http;

    public GamificationApiClient(HttpClient http) => _http = http;

    public async Task<XpSummaryDto?> GetXpSummaryAsync()
    {
        var response = await _http.GetAsync("api/gamification/xp");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<XpSummaryDto>();
    }
}
