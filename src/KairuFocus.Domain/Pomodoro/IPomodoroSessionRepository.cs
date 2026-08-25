using KairuFocus.Domain.Identity;

namespace KairuFocus.Domain.Pomodoro;

public interface IPomodoroSessionRepository
{
    /// <summary>
    /// Adds the session. Returns false when adding this session would violate the
    /// "at most one Active session per owner" invariant (concurrent start).
    /// </summary>
    Task<bool> TryAddAsync(PomodoroSession session, CancellationToken cancellationToken = default);

    Task<PomodoroSession?> GetByIdAsync(PomodoroSessionId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PomodoroSession>> GetByIdsAsync(IEnumerable<PomodoroSessionId> ids, CancellationToken cancellationToken = default);
    Task<PomodoroSession?> GetActiveAsync(UserId userId, CancellationToken cancellationToken = default);
    Task UpdateAsync(PomodoroSession session, CancellationToken cancellationToken = default);
    Task<int> GetCompletedTodayCountAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<PomodoroSession?> GetLatestCompletedTodayAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PomodoroSession>> GetTodaySprintSessionsAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Count of completed sprint sessions whose EndedAt falls within [startUtc, endUtc).
    /// Uses UTC range comparison (provider-safe: no .Date or AddMinutes in SQL).
    /// </summary>
    Task<int> GetCompletedSprintsTodayCountAsync(UserId userId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default);

    /// <summary>
    /// All sprint sessions completed within [startUtc, endUtc).
    /// Covers both free and regular sprints. Used to sum today's focus time.
    /// Uses UTC range comparison (provider-safe).
    /// </summary>
    Task<IReadOnlyList<PomodoroSession>> GetCompletedSprintSessionsTodayAsync(UserId userId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default);

    /// <summary>
    /// EndedAt (UTC) of completed sprint sessions for the user where EndedAt >= sinceUtc.
    /// The Application layer maps these to local dates using the user's UTC offset (ADR-020).
    /// No date bucketing in SQL — safe for both SQL Server and SQLite providers.
    /// The sinceUtc bound limits the result set to a recent window (e.g. last 366 days)
    /// so that the in-memory bucketing does not materialise the user's entire history.
    /// </summary>
    Task<IReadOnlyList<DateTime>> GetCompletedSprintEndTimesAsync(UserId userId, DateTime sinceUtc, CancellationToken cancellationToken = default);

    /// <summary>
    /// (StartedAt, EndedAt) UTC of completed sprint sessions for the user where EndedAt >= sinceUtc.
    /// Covers both free and regular sprints. Used to aggregate daily focus statistics
    /// (sprint counts + focus minutes) in the Application layer (ADR-020/021).
    /// Simple >= comparison is provider-safe (no date bucketing in SQL).
    /// </summary>
    Task<IReadOnlyList<CompletedSprintInterval>> GetCompletedSprintIntervalsAsync(UserId userId, DateTime sinceUtc, CancellationToken cancellationToken = default);
}
