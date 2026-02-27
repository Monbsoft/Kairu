namespace Kairudev.Domain.Pomodoro;

public interface IPomodoroSessionRepository
{
    Task AddAsync(PomodoroSession session, CancellationToken cancellationToken = default);
    Task<PomodoroSession?> GetByIdAsync(PomodoroSessionId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PomodoroSession>> GetByIdsAsync(IEnumerable<PomodoroSessionId> ids, CancellationToken cancellationToken = default);
    Task<PomodoroSession?> GetActiveAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(PomodoroSession session, CancellationToken cancellationToken = default);
    Task<int> GetCompletedTodayCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetCompletedSprintsTodayCountAsync(CancellationToken cancellationToken = default);
    Task<PomodoroSession?> GetLatestCompletedTodayAsync(CancellationToken cancellationToken = default);
}
