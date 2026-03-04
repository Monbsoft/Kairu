using Kairudev.Domain.Identity;

namespace Kairudev.Domain.Pomodoro;

public interface IPomodoroSessionRepository
{
    Task AddAsync(PomodoroSession session, CancellationToken cancellationToken = default);
    Task<PomodoroSession?> GetByIdAsync(PomodoroSessionId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PomodoroSession>> GetByIdsAsync(IEnumerable<PomodoroSessionId> ids, CancellationToken cancellationToken = default);
    Task<PomodoroSession?> GetActiveAsync(UserId userId, CancellationToken cancellationToken = default);
    Task UpdateAsync(PomodoroSession session, CancellationToken cancellationToken = default);
    Task<int> GetCompletedTodayCountAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<int> GetCompletedSprintsTodayCountAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<PomodoroSession?> GetLatestCompletedTodayAsync(UserId userId, CancellationToken cancellationToken = default);
}
