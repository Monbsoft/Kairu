using System.Net.Http.Json;

namespace Kairu.Web.Services;

public sealed record McpTokenStatusDto(bool HasToken, DateTime? ExpiresAt);

public sealed record McpTokenGenerateDto(string RawToken);

public sealed class McpTokenService
{
    private readonly HttpClient _http;

    public McpTokenService(HttpClient http) => _http = http;

    public async Task<McpTokenStatusDto?> GetStatusAsync()
    {
        var response = await _http.GetAsync("api/mcp-token");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<McpTokenStatusDto>();
    }

    public async Task<string?> GenerateAsync()
    {
        var response = await _http.PostAsync("api/mcp-token", null);
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<McpTokenGenerateDto>();
        return result?.RawToken;
    }

    public async Task<bool> RevokeAsync()
    {
        var response = await _http.DeleteAsync("api/mcp-token");
        return response.IsSuccessStatusCode;
    }
}
