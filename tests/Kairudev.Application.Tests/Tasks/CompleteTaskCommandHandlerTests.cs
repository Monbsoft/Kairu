using Kairudev.Application.Journal.Commands.CreateEntry;
using Kairudev.Application.Tasks.Commands.CompleteTask;
using Kairudev.Application.Tests.Common;
using Kairudev.Application.Tests.Journal;
using Kairudev.Domain.Identity;
using Kairudev.Domain.Journal;
using Kairudev.Domain.Tasks;
using DomainTaskStatus = Kairudev.Domain.Tasks.TaskStatus;

namespace Kairudev.Application.Tests.Tasks;

public sealed class CompleteTaskCommandHandlerTests
{
    private readonly FakeTaskRepository _repository = new();
    private readonly FakeJournalEntryRepository _journalRepository = new();
    private readonly CompleteTaskCommandHandler _sut;

    public CompleteTaskCommandHandlerTests()
    {
        var journalHandler = new CreateEntryCommandHandler(_journalRepository);
        _sut = new CompleteTaskCommandHandler(_repository, journalHandler, new FakeCurrentUserService());
    }

    [Fact]
    public async Task Should_ReturnSuccess_When_TaskExistsAndIsPending()
    {
        // Arrange
        var task = CreateAndAddTask();
        var command = new CompleteTaskCommand(task.Id.Value);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(DomainTaskStatus.Done, _repository.Tasks[0].Status);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TaskDoesNotExist()
    {
        // Arrange
        var command = new CompleteTaskCommand(Guid.NewGuid());

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        Assert.True(result.IsNotFound);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_TaskAlreadyCompleted()
    {
        // Arrange
        var task = CreateAndAddTask();
        task.Complete();
        var command = new CompleteTaskCommand(task.Id.Value);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task Should_CreateJournalEntry_When_TaskCompleted()
    {
        // Arrange
        var task = CreateAndAddTask();
        var command = new CompleteTaskCommand(task.Id.Value);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(_journalRepository.Entries);
        Assert.Equal(JournalEventType.TaskCompleted, _journalRepository.Entries[0].EventType);
    }

    private DeveloperTask CreateAndAddTask()
    {
        var task = DeveloperTask.Create(
            TaskTitle.Create("Task to complete").Value,
            null,
            DateTime.UtcNow,
            UserId.From("test-github-id-123"));
        _repository.Tasks.Add(task);
        return task;
    }
}
