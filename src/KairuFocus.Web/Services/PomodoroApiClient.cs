using System.Net;
using System.Net.Http.Json;

namespace KairuFocus.Web.Services;

public sealed class PomodoroApiClient
{
    private readonly HttpClient _http;

    public PomodoroApiClient(HttpClient http) => _http = http;

    // ── Settings ───────────────────────────────────────────────────────────

    public async Task<PomodoroSettingsDto?> GetSettingsAsync()
    {
        var response = await _http.GetAsync("api/pomodoro/settings");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<PomodoroSettingsDto>();
    }

    public async Task<bool> SaveSettingsAsync(int sprint, int shortBreak, int longBreak)
    {
        var response = await _http.PutAsJsonAsync("api/pomodoro/settings", new
        {
            SprintDurationMinutes = sprint,
            ShortBreakDurationMinutes = shortBreak,
            LongBreakDurationMinutes = longBreak
        });
        return response.IsSuccessStatusCode;
    }

    // ── Session ────────────────────────────────────────────────────────────

    public async Task<SuggestedSessionTypeDto?> GetSuggestedSessionTypeAsync()
    {
        var response = await _http.GetAsync("api/pomodoro/session/suggested");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<SuggestedSessionTypeDto>();
    }

    public async Task<PomodoroSessionDto?> GetCurrentSessionAsync()
    {
        var response = await _http.GetAsync("api/pomodoro/session");
        if (response.StatusCode == HttpStatusCode.NoContent) return null;
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<PomodoroSessionDto>();
    }

    public async Task<PomodoroSessionDto?> StartSessionAsync(string? sessionType = null)
    {
        var url = string.IsNullOrEmpty(sessionType)
            ? "api/pomodoro/session"
            : $"api/pomodoro/session?type={sessionType}";
        var response = await _http.PostAsync(url, null);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<PomodoroSessionDto>();
    }

    public async Task<PomodoroSessionDto?> StartFreeSprintAsync(string? journalComment)
    {
        var response = await _http.PostAsJsonAsync("api/pomodoro/session/free-sprint",
            new { JournalComment = journalComment });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<PomodoroSessionDto>();
    }

    public async Task<List<PomodoroSessionDto>> GetTodaySprintSessionsAsync()
    {
        var response = await _http.GetAsync("api/pomodoro/sessions/today-sprints");
        if (!response.IsSuccessStatusCode) return [];
        return await response.Content.ReadFromJsonAsync<List<PomodoroSessionDto>>() ?? [];
    }

    public async Task<bool> CompleteSessionAsync()
    {
        var response = await _http.PatchAsync("api/pomodoro/session/complete", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> InterruptSessionAsync()
    {
        var response = await _http.PatchAsync("api/pomodoro/session/interrupt", null);
        return response.IsSuccessStatusCode;
    }

    // ── Tasks within session ───────────────────────────────────────────────

    public async Task<bool> LinkTaskAsync(Guid taskId)
    {
        var response = await _http.PostAsync($"api/pomodoro/session/tasks/{taskId}", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UnlinkTaskAsync(Guid taskId)
    {
        var response = await _http.DeleteAsync($"api/pomodoro/session/tasks/{taskId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<TaskDto?> CreateTaskDuringSessionAsync(string title)
    {
        var response = await _http.PostAsJsonAsync("api/pomodoro/session/tasks", new { Title = title });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TaskDto>();
    }

    public async Task<TaskDto?> UpdateTaskStatusAsync(Guid taskId, string targetStatus)
    {
        var response = await _http.PatchAsJsonAsync(
            $"api/pomodoro/session/tasks/{taskId}/status",
            new { TargetStatus = targetStatus });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TaskDto>();
    }
}
