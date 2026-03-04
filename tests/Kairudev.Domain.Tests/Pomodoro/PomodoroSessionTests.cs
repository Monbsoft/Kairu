using Kairudev.Domain.Identity;
using Kairudev.Domain.Pomodoro;
using Kairudev.Domain.Tasks;
using PomodoroErrors = Kairudev.Domain.Pomodoro.DomainErrors;

namespace Kairudev.Domain.Tests.Pomodoro;

public sealed class PomodoroSessionTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
    private static readonly UserId OwnerId = UserId.From("test-user");

    private static PomodoroSession CreatePlanned(int minutes = 25) =>
        PomodoroSession.Create(PomodoroSessionType.Sprint, minutes, OwnerId);

    private static PomodoroSession CreateActive(int minutes = 25)
    {
        var s = PomodoroSession.Create(PomodoroSessionType.Sprint, minutes, OwnerId);
        s.Start(Now);
        return s;
    }

    // ── Create ─────────────────────────────────────────────────────────────

    [Fact]
    public void Should_BeInPlannedStatus_When_Created()
    {
        var session = CreatePlanned();

        Assert.Equal(PomodoroSessionStatus.Planned, session.Status);
        Assert.Null(session.StartedAt);
        Assert.Null(session.EndedAt);
        Assert.Empty(session.LinkedTaskIds);
    }

    [Fact]
    public void Should_HaveUniqueId_When_TwoSessionsCreated()
    {
        var s1 = CreatePlanned();
        var s2 = CreatePlanned();

        Assert.NotEqual(s1.Id, s2.Id);
    }

    // ── Start ──────────────────────────────────────────────────────────────

    [Fact]
    public void Should_BeInActiveStatus_When_Started()
    {
        var session = CreatePlanned();

        var result = session.Start(Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(PomodoroSessionStatus.Active, session.Status);
        Assert.Equal(Now, session.StartedAt);
    }

    [Fact]
    public void Should_ReturnFailure_When_StartingAlreadyActiveSession()
    {
        var session = CreateActive();

        var result = session.Start(Now.AddMinutes(5));

        Assert.True(result.IsFailure);
        Assert.Equal(PomodoroErrors.Pomodoro.InvalidTransition, result.Error);
    }

    [Fact]
    public void Should_ReturnFailure_When_StartingCompletedSession()
    {
        var session = CreateActive();
        session.Complete(Now.AddMinutes(25));

        var result = session.Start(Now.AddMinutes(30));

        Assert.True(result.IsFailure);
    }

    // ── Complete ───────────────────────────────────────────────────────────

    [Fact]
    public void Should_BeInCompletedStatus_When_Completed()
    {
        var session = CreateActive();
        var endTime = Now.AddMinutes(25);

        var result = session.Complete(endTime);

        Assert.True(result.IsSuccess);
        Assert.Equal(PomodoroSessionStatus.Completed, session.Status);
        Assert.Equal(endTime, session.EndedAt);
    }

    [Fact]
    public void Should_ReturnFailure_When_CompletingNonActiveSession()
    {
        var session = CreatePlanned();

        var result = session.Complete(Now);

        Assert.True(result.IsFailure);
        Assert.Equal(PomodoroErrors.Pomodoro.SessionNotActive, result.Error);
    }

    // ── Interrupt ──────────────────────────────────────────────────────────

    [Fact]
    public void Should_BeInInterruptedStatus_When_Interrupted()
    {
        var session = CreateActive();
        var endTime = Now.AddMinutes(10);

        var result = session.Interrupt(endTime);

        Assert.True(result.IsSuccess);
        Assert.Equal(PomodoroSessionStatus.Interrupted, session.Status);
        Assert.Equal(endTime, session.EndedAt);
    }

    [Fact]
    public void Should_ReturnFailure_When_InterruptingNonActiveSession()
    {
        var session = CreatePlanned();

        var result = session.Interrupt(Now);

        Assert.True(result.IsFailure);
        Assert.Equal(PomodoroErrors.Pomodoro.SessionNotActive, result.Error);
    }

    // ── LinkTask ───────────────────────────────────────────────────────────

    [Fact]
    public void Should_ContainLinkedTask_When_TaskLinked()
    {
        var session = CreateActive();
        var taskId = TaskId.New();

        var result = session.LinkTask(taskId);

        Assert.True(result.IsSuccess);
        Assert.Contains(taskId, session.LinkedTaskIds);
    }

    [Fact]
    public void Should_ReturnFailure_When_LinkingAlreadyLinkedTask()
    {
        var session = CreateActive();
        var taskId = TaskId.New();
        session.LinkTask(taskId);

        var result = session.LinkTask(taskId);

        Assert.True(result.IsFailure);
        Assert.Equal(PomodoroErrors.Pomodoro.TaskAlreadyLinked, result.Error);
    }

    [Fact]
    public void Should_LinkMultipleTasks()
    {
        var session = CreateActive();
        var id1 = TaskId.New();
        var id2 = TaskId.New();

        session.LinkTask(id1);
        session.LinkTask(id2);

        Assert.Equal(2, session.LinkedTaskIds.Count);
    }
}
