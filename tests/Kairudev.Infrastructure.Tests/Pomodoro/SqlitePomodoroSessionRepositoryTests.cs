using Kairudev.Domain.Identity;
using Kairudev.Domain.Pomodoro;
using Kairudev.Domain.Tasks;
using Kairudev.Infrastructure.Persistence;

namespace Kairudev.Infrastructure.Tests.Pomodoro;

public sealed class SqlitePomodoroSessionRepositoryTests : InfrastructureTestBase
{
    private static readonly UserId OwnerId = UserId.From("test-user");

    private readonly SqlitePomodoroSessionRepository _repository;

    public SqlitePomodoroSessionRepositoryTests()
    {
        _repository = new SqlitePomodoroSessionRepository(Context);
    }

    [Fact]
    public async Task Should_PersistSession_When_Added()
    {
        var session = PomodoroSession.Create(PomodoroSessionType.Sprint, 25, OwnerId);

        await _repository.AddAsync(session);

        var stored = await _repository.GetByIdAsync(session.Id);
        Assert.NotNull(stored);
        Assert.Equal(session.Id, stored.Id);
        Assert.Equal(25, stored.PlannedDurationMinutes);
        Assert.Equal(PomodoroSessionStatus.Planned, stored.Status);
    }

    [Fact]
    public async Task Should_ReturnNull_When_SessionNotFound()
    {
        var result = await _repository.GetByIdAsync(PomodoroSessionId.New());

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_ReturnActiveSession_When_OneIsActive()
    {
        var session = PomodoroSession.Create(PomodoroSessionType.Sprint, 25, OwnerId);
        session.Start(DateTime.UtcNow);
        await _repository.AddAsync(session);

        var active = await _repository.GetActiveAsync(OwnerId);

        Assert.NotNull(active);
        Assert.Equal(session.Id, active.Id);
        Assert.Equal(PomodoroSessionStatus.Active, active.Status);
    }

    [Fact]
    public async Task Should_ReturnNull_When_NoActiveSession()
    {
        var session = PomodoroSession.Create(PomodoroSessionType.Sprint, 25, OwnerId);
        await _repository.AddAsync(session);

        var active = await _repository.GetActiveAsync(OwnerId);

        Assert.Null(active);
    }

    [Fact]
    public async Task Should_PersistStatusChange_When_SessionUpdated()
    {
        var session = PomodoroSession.Create(PomodoroSessionType.Sprint, 25, OwnerId);
        session.Start(DateTime.UtcNow);
        await _repository.AddAsync(session);

        session.Complete(DateTime.UtcNow);
        await _repository.UpdateAsync(session);

        var stored = await _repository.GetByIdAsync(session.Id);
        Assert.NotNull(stored);
        Assert.Equal(PomodoroSessionStatus.Completed, stored.Status);
        Assert.NotNull(stored.EndedAt);
    }

    [Fact]
    public async Task Should_CountOnlyTodayCompleted_When_SessionsFromMultipleDays()
    {
        var sessionToday = PomodoroSession.Create(PomodoroSessionType.Sprint, 25, OwnerId);
        sessionToday.Start(DateTime.UtcNow);
        sessionToday.Complete(DateTime.UtcNow);
        await _repository.AddAsync(sessionToday);

        var count = await _repository.GetCompletedTodayCountAsync(OwnerId);

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Should_ReturnZero_When_NoCompletedSessions()
    {
        var count = await _repository.GetCompletedTodayCountAsync(OwnerId);

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task Should_PersistLinkedTask_When_TaskLinked()
    {
        var session = PomodoroSession.Create(PomodoroSessionType.Sprint, 25, OwnerId);
        var taskId = TaskId.New();
        session.LinkTask(taskId);
        await _repository.AddAsync(session);

        var stored = await _repository.GetByIdAsync(session.Id);
        Assert.NotNull(stored);
        Assert.Single(stored.LinkedTaskIds);
        Assert.Equal(taskId, stored.LinkedTaskIds[0]);
    }
}
