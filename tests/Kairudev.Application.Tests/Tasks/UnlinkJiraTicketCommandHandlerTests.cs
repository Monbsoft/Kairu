using Kairudev.Application.Tasks.Commands.UnlinkJiraTicket;
using Kairudev.Application.Tests.Common;
using Kairudev.Domain.Identity;
using Kairudev.Domain.Tasks;

namespace Kairudev.Application.Tests.Tasks;

public sealed class UnlinkJiraTicketCommandHandlerTests
{
    private readonly FakeTaskRepository _repository = new();
    private readonly UnlinkJiraTicketCommandHandler _handler;

    public UnlinkJiraTicketCommandHandlerTests()
    {
        _handler = new UnlinkJiraTicketCommandHandler(_repository, new FakeCurrentUserService());
    }

    private static DeveloperTask CreateTaskWithJiraLink()
    {
        var title = TaskTitle.Create("Test task").Value;
        var task = DeveloperTask.Create(title, null, DateTime.UtcNow, UserId.From("test-github-id-123"));
        task.LinkJiraTicket(JiraTicketKey.Create("PROJ-123").Value);
        return task;
    }

    [Fact]
    public async Task Should_UnlinkJiraTicket_When_TaskExistsAndTicketIsLinked()
    {
        var task = CreateTaskWithJiraLink();
        _repository.Tasks.Add(task);

        var result = await _handler.HandleAsync(
            new UnlinkJiraTicketCommand(task.Id.Value));

        Assert.True(result.IsSuccess);
        Assert.Null(task.JiraTicketKey);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TaskDoesNotExist()
    {
        var result = await _handler.HandleAsync(
            new UnlinkJiraTicketCommand(Guid.NewGuid()));

        Assert.True(result.IsNotFound);
    }

    [Fact]
    public async Task Should_Succeed_When_TaskHasNoTicketLinked()
    {
        var title = TaskTitle.Create("No jira link").Value;
        var task = DeveloperTask.Create(title, null, DateTime.UtcNow, UserId.From("test-github-id-123"));
        _repository.Tasks.Add(task);

        var result = await _handler.HandleAsync(
            new UnlinkJiraTicketCommand(task.Id.Value));

        Assert.True(result.IsSuccess);
    }
}
