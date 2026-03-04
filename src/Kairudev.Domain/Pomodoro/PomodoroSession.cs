using Kairudev.Domain.Common;
using Kairudev.Domain.Identity;
using Kairudev.Domain.Tasks;

namespace Kairudev.Domain.Pomodoro;

public sealed class PomodoroSession : AggregateRoot<PomodoroSessionId>
{
    private readonly List<Guid> _linkedTaskIdValues = [];

    private PomodoroSession(PomodoroSessionId id, UserId ownerId, PomodoroSessionType sessionType, int plannedDurationMinutes)
        : base(id)
    {
        OwnerId = ownerId;
        SessionType = sessionType;
        PlannedDurationMinutes = plannedDurationMinutes;
        Status = PomodoroSessionStatus.Planned;
    }

    public UserId OwnerId { get; private set; } = default!;
    public PomodoroSessionType SessionType { get; }
    public PomodoroSessionStatus Status { get; private set; }
    public int PlannedDurationMinutes { get; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }

    public IReadOnlyList<TaskId> LinkedTaskIds =>
        _linkedTaskIdValues.ConvertAll(TaskId.From).AsReadOnly();

    // Public for EF Core materialization — domain consumers use LinkedTaskIds
    public List<Guid> LinkedTaskIdValues
    {
        get => _linkedTaskIdValues;
        init => _linkedTaskIdValues = value;
    }

    public static PomodoroSession Create(PomodoroSessionType sessionType, int plannedDurationMinutes, UserId ownerId) =>
        new(PomodoroSessionId.New(), ownerId, sessionType, plannedDurationMinutes);

    public Result Start(DateTime now)
    {
        if (Status != PomodoroSessionStatus.Planned)
            return Result.Failure(DomainErrors.Pomodoro.InvalidTransition);

        Status = PomodoroSessionStatus.Active;
        StartedAt = now;
        return Result.Success();
    }

    public Result Complete(DateTime now)
    {
        if (Status != PomodoroSessionStatus.Active)
            return Result.Failure(DomainErrors.Pomodoro.SessionNotActive);

        Status = PomodoroSessionStatus.Completed;
        EndedAt = now;
        return Result.Success();
    }

    public Result Interrupt(DateTime now)
    {
        if (Status != PomodoroSessionStatus.Active)
            return Result.Failure(DomainErrors.Pomodoro.SessionNotActive);

        Status = PomodoroSessionStatus.Interrupted;
        EndedAt = now;
        return Result.Success();
    }

    public Result LinkTask(TaskId taskId)
    {
        if (_linkedTaskIdValues.Contains(taskId.Value))
            return Result.Failure(DomainErrors.Pomodoro.TaskAlreadyLinked);

        _linkedTaskIdValues.Add(taskId.Value);
        return Result.Success();
    }
}
