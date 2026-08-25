using KairuFocus.Domain.Identity;
using KairuFocus.Domain.Pomodoro;

namespace KairuFocus.Application.Tests.Pomodoro;

internal sealed class FakePomodoroSessionRepository : IPomodoroSessionRepository
{
    public List<PomodoroSession> Sessions { get; } = [];
    public int UpdateAsyncCallCount { get; private set; }

    /// <summary>Invoked at the beginning of TryAddAsync — lets a test insert the "winning"
    /// concurrent session, faithfully reproducing the unique-index rejection.</summary>
    public Action? OnTryAdd { get; set; }

    /// <summary>Invoked when TryAddAsync rejects the insert — lets a test close the winning
    /// session in between, reproducing the "rejected but nothing readable" race.</summary>
    public Action? OnInsertRejected { get; set; }

    /// <summary>
    /// Simulates the unique filtered index: the insert is rejected when another Active
    /// session already exists for the same owner.
    /// </summary>
    public Task<bool> TryAddAsync(PomodoroSession session, CancellationToken cancellationToken = default)
    {
        OnTryAdd?.Invoke();

        var conflicts = Sessions.Any(s => s.OwnerId == session.OwnerId
                                          && s.Status == PomodoroSessionStatus.Active);
        if (conflicts)
        {
            OnInsertRejected?.Invoke();
            return Task.FromResult(false);
        }

        Sessions.Add(session);
        return Task.FromResult(true);
    }

    public Task<PomodoroSession?> GetByIdAsync(PomodoroSessionId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Sessions.FirstOrDefault(s => s.Id == id));

    public Task<IReadOnlyList<PomodoroSession>> GetByIdsAsync(IEnumerable<PomodoroSessionId> ids, CancellationToken cancellationToken = default)
    {
        var idSet = ids.ToHashSet();
        return Task.FromResult<IReadOnlyList<PomodoroSession>>(Sessions.Where(s => idSet.Contains(s.Id)).ToList());
    }

    public Task<PomodoroSession?> GetActiveAsync(UserId userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Sessions.FirstOrDefault(s => s.OwnerId == userId && s.Status == PomodoroSessionStatus.Active));

    public Task UpdateAsync(PomodoroSession session, CancellationToken cancellationToken = default)
    {
        UpdateAsyncCallCount++;
        return Task.CompletedTask;
    }

    public Task<int> GetCompletedTodayCountAsync(UserId userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Sessions.Count(s => s.OwnerId == userId
                                            && s.Status == PomodoroSessionStatus.Completed
                                            && s.EndedAt.HasValue
                                            && s.EndedAt.Value.Date == DateTime.UtcNow.Date));

    public Task<int> GetCompletedSprintsTodayCountAsync(UserId userId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default) =>
        Task.FromResult(Sessions.Count(s => s.OwnerId == userId
                                            && s.SessionType == PomodoroSessionType.Sprint
                                            && s.Status == PomodoroSessionStatus.Completed
                                            && s.EndedAt.HasValue
                                            && s.EndedAt.Value >= startUtc
                                            && s.EndedAt.Value < endUtc));

    public Task<PomodoroSession?> GetLatestCompletedTodayAsync(UserId userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Sessions
            .Where(s => s.OwnerId == userId && s.Status == PomodoroSessionStatus.Completed && s.EndedAt.HasValue && s.EndedAt.Value.Date == DateTime.UtcNow.Date)
            .OrderByDescending(s => s.EndedAt)
            .FirstOrDefault());

    public Task<IReadOnlyList<PomodoroSession>> GetTodaySprintSessionsAsync(UserId userId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<PomodoroSession>>(Sessions
            .Where(s => s.OwnerId == userId
                        && s.SessionType == PomodoroSessionType.Sprint
                        && s.PlannedDurationMinutes == 0
                        && s.StartedAt.HasValue
                        && s.StartedAt.Value.Date == DateTime.UtcNow.Date
                        && (s.Status == PomodoroSessionStatus.Completed
                            || s.Status == PomodoroSessionStatus.Interrupted))
            .OrderBy(s => s.StartedAt)
            .ToList());

    public Task<IReadOnlyList<PomodoroSession>> GetCompletedSprintSessionsTodayAsync(UserId userId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<PomodoroSession>>(Sessions
            .Where(s => s.OwnerId == userId
                        && s.SessionType == PomodoroSessionType.Sprint
                        && s.Status == PomodoroSessionStatus.Completed
                        && s.EndedAt.HasValue
                        && s.EndedAt.Value >= startUtc
                        && s.EndedAt.Value < endUtc)
            .OrderBy(s => s.StartedAt)
            .ToList());

    public Task<IReadOnlyList<DateTime>> GetCompletedSprintEndTimesAsync(UserId userId, DateTime sinceUtc, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<DateTime>>(Sessions
            .Where(s => s.OwnerId == userId
                        && s.SessionType == PomodoroSessionType.Sprint
                        && s.Status == PomodoroSessionStatus.Completed
                        && s.EndedAt.HasValue
                        && s.EndedAt.Value >= sinceUtc)
            .Select(s => s.EndedAt!.Value)
            .ToList());

    public Task<IReadOnlyList<CompletedSprintInterval>> GetCompletedSprintIntervalsAsync(UserId userId, DateTime sinceUtc, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<CompletedSprintInterval>>(Sessions
            .Where(s => s.OwnerId == userId
                        && s.SessionType == PomodoroSessionType.Sprint
                        && s.Status == PomodoroSessionStatus.Completed
                        && s.StartedAt.HasValue
                        && s.EndedAt.HasValue
                        && s.EndedAt.Value >= sinceUtc)
            .Select(s => new CompletedSprintInterval(s.StartedAt!.Value, s.EndedAt!.Value))
            .ToList());
}
