using Kairudev.Domain.Identity;
using Kairudev.Domain.Tasks;

namespace Kairudev.Domain.Tests.Tasks;

public sealed class DeveloperTaskUpdateDetailsTests
{
    private static readonly UserId OwnerId = UserId.From("test-user");

    [Fact]
    public void Should_UpdateTitle_When_UpdateDetailsIsCalled()
    {
        var task = DeveloperTask.Create(
            TaskTitle.Create("Original title").Value,
            null,
            DateTime.UtcNow,
            OwnerId);
        var newTitle = TaskTitle.Create("Updated title").Value;

        task.UpdateDetails(newTitle, null);

        Assert.Equal("Updated title", task.Title.Value);
    }

    [Fact]
    public void Should_UpdateDescription_When_UpdateDetailsIsCalled()
    {
        var task = DeveloperTask.Create(
            TaskTitle.Create("Task title").Value,
            null,
            DateTime.UtcNow,
            OwnerId);
        var newDescription = TaskDescription.Create("New description").Value;

        task.UpdateDetails(task.Title, newDescription);

        Assert.NotNull(task.Description);
        Assert.Equal("New description", task.Description.Value);
    }

    [Fact]
    public void Should_UpdateBothTitleAndDescription_When_UpdateDetailsIsCalled()
    {
        var task = DeveloperTask.Create(
            TaskTitle.Create("Original title").Value,
            TaskDescription.Create("Original description").Value,
            DateTime.UtcNow,
            OwnerId);
        var newTitle = TaskTitle.Create("Updated title").Value;
        var newDescription = TaskDescription.Create("Updated description").Value;

        task.UpdateDetails(newTitle, newDescription);

        Assert.Equal("Updated title", task.Title.Value);
        Assert.NotNull(task.Description);
        Assert.Equal("Updated description", task.Description.Value);
    }

    [Fact]
    public void Should_RemoveDescription_When_UpdateDetailsIsCalledWithNull()
    {
        var task = DeveloperTask.Create(
            TaskTitle.Create("Task title").Value,
            TaskDescription.Create("Original description").Value,
            DateTime.UtcNow,
            OwnerId);

        task.UpdateDetails(task.Title, null);

        Assert.Null(task.Description);
    }

    [Fact]
    public void Should_CreateTaskWithDescription_When_DescriptionIsProvided()
    {
        var title = TaskTitle.Create("Task with description").Value;
        var description = TaskDescription.Create("This is a description").Value;

        var task = DeveloperTask.Create(title, description, DateTime.UtcNow, OwnerId);

        Assert.Equal("Task with description", task.Title.Value);
        Assert.NotNull(task.Description);
        Assert.Equal("This is a description", task.Description.Value);
    }

    [Fact]
    public void Should_CreateTaskWithoutDescription_When_DescriptionIsNull()
    {
        var title = TaskTitle.Create("Task without description").Value;

        var task = DeveloperTask.Create(title, null, DateTime.UtcNow, OwnerId);

        Assert.Equal("Task without description", task.Title.Value);
        Assert.Null(task.Description);
    }
}
