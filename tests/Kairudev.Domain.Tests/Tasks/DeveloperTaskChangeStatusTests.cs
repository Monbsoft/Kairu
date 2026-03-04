using Kairudev.Domain.Identity;
using Kairudev.Domain.Tasks;
using DomainTaskStatus = Kairudev.Domain.Tasks.TaskStatus;

namespace Kairudev.Domain.Tests.Tasks;

public sealed class DeveloperTaskChangeStatusTests
{
    private static readonly DateTime Now = new(2026, 2, 26, 10, 0, 0, DateTimeKind.Utc);
    private static readonly UserId OwnerId = UserId.From("test-user");

    private static DeveloperTask CreatePendingTask() =>
        DeveloperTask.Create(TaskTitle.Create("Test task").Value, null, Now, OwnerId);

    private static DeveloperTask CreateInProgressTask()
    {
        var task = CreatePendingTask();
        task.ChangeStatus(DomainTaskStatus.InProgress, Now);
        return task;
    }

    private static DeveloperTask CreateDoneTask()
    {
        var task = CreatePendingTask();
        task.ChangeStatus(DomainTaskStatus.Done, Now);
        return task;
    }

    [Fact]
    public void Should_ReturnSuccess_When_StatusChangeFromPendingToInProgress()
    {
        var task = CreatePendingTask();

        var result = task.ChangeStatus(DomainTaskStatus.InProgress, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(DomainTaskStatus.InProgress, task.Status);
    }

    [Fact]
    public void Should_ReturnSuccess_When_StatusChangeFromInProgressToDone()
    {
        var task = CreateInProgressTask();

        var result = task.ChangeStatus(DomainTaskStatus.Done, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(DomainTaskStatus.Done, task.Status);
    }

    [Fact]
    public void Should_ReturnSuccess_When_StatusChangeFromDoneToPending()
    {
        var task = CreateDoneTask();

        var result = task.ChangeStatus(DomainTaskStatus.Pending, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(DomainTaskStatus.Pending, task.Status);
    }

    [Fact]
    public void Should_ReturnSuccess_When_StatusChangeFromDoneToInProgress()
    {
        var task = CreateDoneTask();

        var result = task.ChangeStatus(DomainTaskStatus.InProgress, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(DomainTaskStatus.InProgress, task.Status);
    }

    [Fact]
    public void Should_SetCompletedAt_When_StatusChangedToDone()
    {
        var task = CreatePendingTask();
        var completionTime = new DateTime(2026, 2, 26, 14, 30, 0, DateTimeKind.Utc);

        task.ChangeStatus(DomainTaskStatus.Done, completionTime);

        Assert.Equal(completionTime, task.CompletedAt);
    }

    [Fact]
    public void Should_ClearCompletedAt_When_StatusChangedFromDone()
    {
        var task = CreateDoneTask();

        task.ChangeStatus(DomainTaskStatus.Pending, Now);

        Assert.Null(task.CompletedAt);
    }

    [Fact]
    public void Should_ReturnFailure_When_StatusIsSame_Pending()
    {
        var task = CreatePendingTask();

        var result = task.ChangeStatus(DomainTaskStatus.Pending, Now);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Tasks.SameStatus, result.Error);
    }

    [Fact]
    public void Should_ReturnFailure_When_StatusIsSame_InProgress()
    {
        var task = CreateInProgressTask();

        var result = task.ChangeStatus(DomainTaskStatus.InProgress, Now);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Tasks.SameStatus, result.Error);
    }

    [Fact]
    public void Should_ReturnFailure_When_StatusIsSame_Done()
    {
        var task = CreateDoneTask();

        var result = task.ChangeStatus(DomainTaskStatus.Done, Now);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Tasks.SameStatus, result.Error);
    }
}
