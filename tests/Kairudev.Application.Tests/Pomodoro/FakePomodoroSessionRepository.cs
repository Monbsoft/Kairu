using Kairudev.Domain.Pomodoro;

namespace Kairudev.Application.Tests.Pomodoro;

internal sealed class FakePomodoroSessionRepository : IPomodoroSessionRepository
{
    public List<PomodoroSession> Sessions { get; } = [];

    public Task AddAsync(PomodoroSession session, CancellationToken cancellationToken = default)
    {
        Sessions.Add(session);
        return Task.CompletedTask;
    }

    public Task<PomodoroSession?> GetByIdAsync(PomodoroSessionId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Sessions.FirstOrDefault(s => s.Id == id));

    public Task<IReadOnlyList<PomodoroSession>> GetByIdsAsync(IEnumerable<PomodoroSessionId> ids, CancellationToken cancellationToken = default)
    {
        var idSet = ids.ToHashSet();
        return Task.FromResult<IReadOnlyList<PomodoroSession>>(Sessions.Where(s => idSet.Contains(s.Id)).ToList());
    }

    public Task<PomodoroSession?> GetActiveAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Sessions.FirstOrDefault(s => s.Status == PomodoroSessionStatus.Active));

    public Task UpdateAsync(PomodoroSession session, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task<int> GetCompletedTodayCountAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Sessions.Count(s => s.Status == PomodoroSessionStatus.Completed));

    public Task<int> GetCompletedSprintsTodayCountAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Sessions.Count(s => s.SessionType == PomodoroSessionType.Sprint && s.Status == PomodoroSessionStatus.Completed));

    public Task<PomodoroSession?> GetLatestCompletedTodayAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Sessions
            .Where(s => s.Status == PomodoroSessionStatus.Completed && s.EndedAt.HasValue && s.EndedAt.Value.Date == DateTime.UtcNow.Date)
            .OrderByDescending(s => s.EndedAt)
            .FirstOrDefault());
}
