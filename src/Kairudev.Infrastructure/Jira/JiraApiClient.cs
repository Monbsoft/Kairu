using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Kairudev.Application.Tickets;
using Kairudev.Domain.Common;

namespace Kairudev.Infrastructure.Jira;

public sealed class JiraApiClient : IJiraTicketService
{
    private readonly HttpClient _httpClient;

    public JiraApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<IReadOnlyList<JiraTicketDto>>> GetAssignedTicketsAsync(
        string baseUrl, string email, string apiToken,
        CancellationToken cancellationToken = default)
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{email}:{apiToken}"));
        var request = new HttpRequestMessage(HttpMethod.Get,
            $"{baseUrl.TrimEnd('/')}/rest/api/3/search?jql=assignee%3DcurrentUser()%20ORDER%20BY%20updated%20DESC&fields=summary,status,priority&maxResults=50");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<IReadOnlyList<JiraTicketDto>>($"Network error: {ex.Message}");
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return Result.Failure<IReadOnlyList<JiraTicketDto>>(
                $"Jira API error {(int)response.StatusCode}: {body}");
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var doc = JsonDocument.Parse(json);
        var issues = doc.RootElement.GetProperty("issues");

        var tickets = new List<JiraTicketDto>();
        foreach (var issue in issues.EnumerateArray())
        {
            var key = issue.GetProperty("key").GetString() ?? "";
            var fields = issue.GetProperty("fields");
            var summary = fields.GetProperty("summary").GetString() ?? "";
            var status = fields.GetProperty("status").GetProperty("name").GetString() ?? "";
            string? priority = null;
            if (fields.TryGetProperty("priority", out var priorityEl) && priorityEl.ValueKind != JsonValueKind.Null)
                priority = priorityEl.GetProperty("name").GetString();

            tickets.Add(new JiraTicketDto(key, summary, status, priority));
        }

        return Result.Success<IReadOnlyList<JiraTicketDto>>(tickets);
    }
}
