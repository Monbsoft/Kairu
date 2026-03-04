using Kairudev.Application.Tasks.Commands.AddTask;
using Kairudev.Application.Tasks.Common;
using Kairudev.Application.Tests.Common;
using Kairudev.Domain.Tasks;

namespace Kairudev.Application.Tests.Tasks;

public sealed class AddTaskCommandHandlerTests
{
    private readonly FakeTaskRepository _repository = new();
    private readonly AddTaskCommandHandler _sut;

    public AddTaskCommandHandlerTests() =>
        _sut = new AddTaskCommandHandler(_repository, new FakeCurrentUserService());

    [Fact]
    public async Task Should_ReturnSuccess_When_TitleIsValid()
    {
        // Arrange
        var command = new AddTaskCommand("Write documentation", null);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Task);
        Assert.Equal("Write documentation", result.Task.Title);
        Assert.Equal("Pending", result.Task.Status);
    }

    [Fact]
    public async Task Should_PersistTask_When_TitleIsValid()
    {
        // Arrange
        var command = new AddTaskCommand("Write documentation", null);

        // Act
        await _sut.HandleAsync(command);

        // Assert
        Assert.Single(_repository.Tasks);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Should_ReturnValidationError_When_TitleIsEmpty(string title)
    {
        // Arrange
        var command = new AddTaskCommand(title, null);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Empty(_repository.Tasks);
    }

    [Fact]
    public async Task Should_ReturnValidationError_When_TitleTooLong()
    {
        // Arrange
        var longTitle = new string('a', TaskTitle.MaxLength + 1);
        var command = new AddTaskCommand(longTitle, null);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
    }
}
