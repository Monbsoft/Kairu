using System.Net.Http.Json;

namespace Kairudev.Web.Services;

public sealed class JournalApiClient
{
    private readonly HttpClient _http;

    public JournalApiClient(HttpClient http) => _http = http;

    public async Task<List<JournalEntryDto>> GetTodayEntriesAsync()
    {
        var response = await _http.GetAsync("api/journal/today");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<JournalEntryDto>>()
            ?? [];
    }

    public async Task<List<JournalEntryDto>> GetEntriesByDateAsync(DateOnly date)
    {
        var response = await _http.GetAsync($"api/journal/date/{date:yyyy-MM-dd}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<JournalEntryDto>>()
            ?? [];
    }

    public async Task<Guid> AddCommentAsync(Guid entryId, string text)
    {
        var body = new AddCommentRequest(text);
        var response = await _http.PostAsJsonAsync($"api/journal/{entryId}/comments", body);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AddCommentResponse>();
        return result!.CommentId;
    }

    public async Task UpdateCommentAsync(Guid entryId, Guid commentId, string text)
    {
        var body = new UpdateCommentRequest(text);
        var response = await _http.PutAsJsonAsync($"api/journal/{entryId}/comments/{commentId}", body);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteCommentAsync(Guid entryId, Guid commentId)
    {
        var response = await _http.DeleteAsync($"api/journal/{entryId}/comments/{commentId}");
        response.EnsureSuccessStatusCode();
    }
}

internal sealed record AddCommentResponse(Guid CommentId);
