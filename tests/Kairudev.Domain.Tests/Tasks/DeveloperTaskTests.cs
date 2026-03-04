using Kairudev.Domain.Identity;
using Kairudev.Domain.Tasks;
using DomainTaskStatus = Kairudev.Domain.Tasks.TaskStatus;

namespace Kairudev.Domain.Tests.Tasks;

public sealed class DeveloperTaskTests
{
    private static readonly UserId OwnerId = UserId.From("test-user");

    private static DeveloperTask CreateValidTask(string title = "Write unit tests") =>
        DeveloperTask.Create(TaskTitle.Create(title).Value, null, DateTime.UtcNow, OwnerId);

    [Fact]
    public void Should_BeInPendingStatus_When_Created()
    {
        var task = CreateValidTask();

        Assert.Equal(DomainTaskStatus.Pending, task.Status);
        Assert.Null(task.CompletedAt);
    }

    [Fact]
    public void Should_BeInDoneStatus_When_Completed()
    {
        var task = CreateValidTask();

        var result = task.Complete();

        Assert.True(result.IsSuccess);
        Assert.Equal(DomainTaskStatus.Done, task.Status);
        Assert.NotNull(task.CompletedAt);
    }

    [Fact]
    public void Should_ReturnFailure_When_CompletingAlreadyCompletedTask()
    {
        var task = CreateValidTask();
        task.Complete();

        var result = task.Complete();

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Tasks.AlreadyCompleted, result.Error);
    }

    [Fact]
    public void Should_BeInProgressStatus_When_StartProgressCalled()
    {
        var task = CreateValidTask();

        var result = task.StartProgress();

        Assert.True(result.IsSuccess);
        Assert.Equal(DomainTaskStatus.InProgress, task.Status);
    }

    [Fact]
    public void Should_ReturnFailure_When_StartingProgressOnCompletedTask()
    {
        var task = CreateValidTask();
        task.Complete();

        var result = task.StartProgress();

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Tasks.AlreadyCompleted, result.Error);
    }

    [Fact]
    public void Should_HaveUniqueId_When_TwoTasksCreated()
    {
        var task1 = CreateValidTask();
        var task2 = CreateValidTask();

        Assert.NotEqual(task1.Id, task2.Id);
    }
}
