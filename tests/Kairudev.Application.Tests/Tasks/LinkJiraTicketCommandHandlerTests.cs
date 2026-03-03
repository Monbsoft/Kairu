using Kairudev.Application.Tasks.Commands.LinkJiraTicket;
using Kairudev.Domain.Tasks;

namespace Kairudev.Application.Tests.Tasks;

public sealed class LinkJiraTicketCommandHandlerTests
{
    private readonly FakeTaskRepository _repository = new();
    private readonly LinkJiraTicketCommandHandler _handler;

    public LinkJiraTicketCommandHandlerTests()
    {
        _handler = new LinkJiraTicketCommandHandler(_repository);
    }

    private static DeveloperTask CreateTask()
    {
        var title = TaskTitle.Create("Test task").Value;
        return DeveloperTask.Create(title, null, DateTime.UtcNow);
    }

    [Fact]
    public async Task Should_LinkJiraTicket_When_TaskExistsAndKeyIsValid()
    {
        var task = CreateTask();
        _repository.Tasks.Add(task);

        var result = await _handler.HandleAsync(
            new LinkJiraTicketCommand(task.Id.Value, "PROJ-123"));

        Assert.True(result.IsSuccess);
        Assert.Equal("PROJ-123", task.JiraTicketKey!.Value);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TaskDoesNotExist()
    {
        var result = await _handler.HandleAsync(
            new LinkJiraTicketCommand(Guid.NewGuid(), "PROJ-123"));

        Assert.True(result.IsNotFound);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("proj-123")]
    public async Task Should_ReturnFailure_When_JiraKeyFormatIsInvalid(string invalidKey)
    {
        var task = CreateTask();
        _repository.Tasks.Add(task);

        var result = await _handler.HandleAsync(
            new LinkJiraTicketCommand(task.Id.Value, invalidKey));

        Assert.False(result.IsSuccess);
        Assert.False(result.IsNotFound);
        Assert.NotNull(result.Error);
    }
}
