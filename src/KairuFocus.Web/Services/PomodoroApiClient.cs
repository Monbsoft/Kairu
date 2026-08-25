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

    public async Task<bool> SaveSettingsAsync(int sprint, int shortBreak, int longBreak, int dailySprintGoal)
    {
        var response = await _http.PutAsJsonAsync("api/pomodoro/settings", new
        {
            SprintDurationMinutes = sprint,
            ShortBreakDurationMinutes = shortBreak,
            LongBreakDurationMinutes = longBreak,
            DailySprintGoal = dailySprintGoal
        });
        return response.IsSuccessStatusCode;
    }

    // ── Focus / Dashboard ──────────────────────────────────────────────────

    public async Task<FocusSummaryDto?> GetFocusSummaryAsync()
    {
        // DateTimeOffset.Now.Offset reflects the browser's local UTC offset in Blazor WASM.
        // Convention: local = UTC + offsetMinutes (positive east of UTC, e.g. UTC+2 => +120).
        var offsetMinutes = (int)DateTimeOffset.Now.Offset.TotalMinutes;
        var response = await _http.GetAsync($"api/pomodoro/focus-summary?offsetMinutes={offsetMinutes}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<FocusSummaryDto>();
    }

    public async Task<FocusStatsDto?> GetFocusStatsAsync()
    {
        // local = UTC + offsetMinutes (cf. GetFocusSummaryAsync).
        var offsetMinutes = (int)DateTimeOffset.Now.Offset.TotalMinutes;
        var response = await _http.GetAsync($"api/pomodoro/focus-stats?offsetMinutes={offsetMinutes}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<FocusStatsDto>();
    }

    // ── Session ────────────────────────────────────────────────────────────

    public async Task<SuggestedSessionTypeDto?> GetSuggestedSessionTypeAsync()
    {
        // DateTimeOffset.Now.Offset reflects the browser's local UTC offset in Blazor WASM.
        var offsetMinutes = (int)DateTimeOffset.Now.Offset.TotalMinutes;
        var response = await _http.GetAsync($"api/pomodoro/session/suggested?offsetMinutes={offsetMinutes}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<SuggestedSessionTypeDto>();
    }

    /// <summary>Fetches the currently active session, if any. Returns <c>null</c> both when there
    /// genuinely isn't one (204/non-success status) and when the call itself failed — no network,
    /// a timeout, or an unparsable body. This mirrors <see cref="ToStartSessionOutcomeAsync"/>: the
    /// resync path in <c>Pomodoro.razor</c>/<c>SprintLibre.razor</c> calls this method precisely
    /// when the network is already suspect (after a start-session call came back with no session),
    /// so it must never let an exception escape — there is no <c>ErrorBoundary</c> in this app, and
    /// an unhandled exception here would replace the page's own error message with the global error
    /// UI.</summary>
    public async Task<PomodoroSessionDto?> GetCurrentSessionAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/pomodoro/session");
            if (response.StatusCode == HttpStatusCode.NoContent) return null;
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<PomodoroSessionDto>();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            return null;
        }
    }

    public async Task<StartSessionOutcome> StartSessionAsync(string? sessionType = null)
    {
        var url = string.IsNullOrEmpty(sessionType)
            ? "api/pomodoro/session"
            : $"api/pomodoro/session?type={sessionType}";

        try
        {
            var response = await _http.PostAsync(url, null);
            return await ToStartSessionOutcomeAsync(response);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return new StartSessionOutcome(null, "Serveur injoignable, vérifie ta connexion.");
        }
    }

    public async Task<StartSessionOutcome> StartFreeSprintAsync(string? journalComment)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/pomodoro/session/free-sprint",
                new { JournalComment = journalComment });
            return await ToStartSessionOutcomeAsync(response);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return new StartSessionOutcome(null, "Serveur injoignable, vérifie ta connexion.");
        }
    }

    /// <summary>Translates a start-session/start-free-sprint HTTP response into a user-facing
    /// outcome. Never throws — a malformed/absent error body falls back to a generic message for
    /// the status code, and a malformed success body (a session was created server-side, but we
    /// can't parse it) is reported as a null session with a generic error rather than propagating
    /// a <see cref="JsonException"/>. This is deliberate: callers resync via
    /// <see cref="GetCurrentSessionAsync"/> whenever <see cref="StartSessionOutcome.Session"/> is
    /// null, so this path still recovers instead of leaving the caller with an unhandled
    /// exception.</summary>
    private static async Task<StartSessionOutcome> ToStartSessionOutcomeAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            try
            {
                var session = await response.Content.ReadFromJsonAsync<PomodoroSessionDto>();
                return new StartSessionOutcome(session, null);
            }
            catch (System.Text.Json.JsonException)
            {
                return new StartSessionOutcome(null, "Réponse du serveur invalide, réessaie.");
            }
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            // The API returns 409 for two distinct causes (SessionAlreadyActive and
            // ConcurrentSessionStart) but only carries their English domain text, which is not
            // displayable here. Both are surfaced with the same message until the API exposes a
            // machine-readable error code — see the technical debt entry for iteration #39.
            return new StartSessionOutcome(null, "Une session est déjà en cours.");
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return new StartSessionOutcome(null, "Ta session a expiré, reconnecte-toi.");

        if ((int)response.StatusCode >= 500)
            return new StartSessionOutcome(null, $"Le serveur n'a pas pu démarrer la session (erreur {(int)response.StatusCode}).");

        return new StartSessionOutcome(null, $"Impossible de démarrer la session (erreur {(int)response.StatusCode}).");
    }

    public async Task<List<PomodoroSessionDto>> GetTodaySprintSessionsAsync()
    {
        var response = await _http.GetAsync("api/pomodoro/sessions/today-sprints");
        if (!response.IsSuccessStatusCode) return [];
        return await response.Content.ReadFromJsonAsync<List<PomodoroSessionDto>>() ?? [];
    }

    /// <summary>Completes the current session. Returns null on failure, otherwise the XP
    /// awarded for this completion (0 if the session wasn't XP-eligible).</summary>
    public async Task<int?> CompleteSessionAsync()
    {
        var response = await _http.PatchAsync("api/pomodoro/session/complete", null);
        if (!response.IsSuccessStatusCode) return null;

        try
        {
            var body = await response.Content.ReadFromJsonAsync<CompleteSessionResponseDto>();
            return body?.XpAwarded ?? 0;
        }
        catch (Exception)
        {
            // Backward-compatible: an absent/malformed body must never fail the completion flow.
            return 0;
        }
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
