using System.Net.Http.Json;

namespace Kairudev.Web.Services;

public sealed class TicketApiClient
{
    private readonly HttpClient _http;

    public TicketApiClient(HttpClient http) => _http = http;

    public async Task<(bool NotConfigured, List<JiraTicketDto> Tickets)> GetAssignedAsync()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<AssignedTicketsResponse>("api/tickets/assigned");
            if (response?.NotConfigured == true) return (true, []);
            return (false, response?.Tickets ?? []);
        }
        catch { return (false, []); }
    }

    public async Task<bool> LinkAsync(Guid taskId, string jiraKey)
    {
        var response = await _http.PutAsJsonAsync($"api/tasks/{taskId}/jira-ticket", new { JiraTicketKey = jiraKey });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UnlinkAsync(Guid taskId)
    {
        var response = await _http.DeleteAsync($"api/tasks/{taskId}/jira-ticket");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SaveJiraSettingsAsync(string? baseUrl, string? email, string? apiToken)
    {
        var response = await _http.PutAsJsonAsync("api/settings/jira",
            new { JiraBaseUrl = baseUrl, JiraEmail = email, JiraApiToken = apiToken });
        return response.IsSuccessStatusCode;
    }
}

public sealed record JiraTicketDto(string Key, string Summary, string Status, string? Priority);
public sealed record AssignedTicketsResponse(bool NotConfigured, List<JiraTicketDto> Tickets);
