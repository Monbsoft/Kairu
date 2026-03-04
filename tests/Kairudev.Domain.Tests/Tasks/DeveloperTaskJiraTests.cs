using Kairudev.Domain.Identity;
using Kairudev.Domain.Tasks;

namespace Kairudev.Domain.Tests.Tasks;

public sealed class DeveloperTaskJiraTests
{
    private static readonly UserId OwnerId = UserId.From("test-user");

    private static DeveloperTask CreateTask() =>
        DeveloperTask.Create(
            TaskTitle.Create("Task title").Value,
            null,
            DateTime.UtcNow,
            OwnerId);

    [Fact]
    public void Should_HaveNoJiraTicket_When_TaskIsCreated()
    {
        var task = CreateTask();

        Assert.Null(task.JiraTicketKey);
    }

    [Fact]
    public void Should_LinkJiraTicket_When_KeyIsValid()
    {
        var task = CreateTask();
        var key = JiraTicketKey.Create("PROJ-123").Value;

        task.LinkJiraTicket(key);

        Assert.NotNull(task.JiraTicketKey);
        Assert.Equal("PROJ-123", task.JiraTicketKey.Value);
    }

    [Fact]
    public void Should_ReplaceJiraTicket_When_AlreadyLinked()
    {
        var task = CreateTask();
        task.LinkJiraTicket(JiraTicketKey.Create("PROJ-1").Value);

        task.LinkJiraTicket(JiraTicketKey.Create("PROJ-2").Value);

        Assert.Equal("PROJ-2", task.JiraTicketKey!.Value);
    }

    [Fact]
    public void Should_UnlinkJiraTicket_When_KeyIsLinked()
    {
        var task = CreateTask();
        task.LinkJiraTicket(JiraTicketKey.Create("PROJ-123").Value);

        task.UnlinkJiraTicket();

        Assert.Null(task.JiraTicketKey);
    }

    [Fact]
    public void Should_DoNothing_When_UnlinkCalledWithNoTicketLinked()
    {
        var task = CreateTask();

        task.UnlinkJiraTicket();

        Assert.Null(task.JiraTicketKey);
    }
}
