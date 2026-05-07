using KairuFocus.Application.Tasks.Queries.ListTasks;
using KairuFocus.Application.Tests.Common;
using KairuFocus.Domain.Tasks;
using Microsoft.Extensions.Logging.Abstractions;

using DomainTaskStatus = KairuFocus.Domain.Tasks.TaskStatus;

namespace KairuFocus.Application.Tests.Tasks;

public sealed class ListTasksQueryHandlerTests
{
    private readonly FakeTaskRepository _repository = new();
    private readonly FakeCurrentUserService _userService = new();
    private readonly ListTasksQueryHandler _sut;

    public ListTasksQueryHandlerTests() =>
        _sut = new ListTasksQueryHandler(_repository, _userService, NullLogger<ListTasksQueryHandler>.Instance);

    private DeveloperTask CreateTask(string title, DomainTaskStatus status = DomainTaskStatus.Pending, DateTime? createdAt = null)
    {
        var taskTitle = TaskTitle.Create(title).Value;
        var task = DeveloperTask.Create(taskTitle, null, createdAt ?? DateTime.UtcNow, _userService.CurrentUserId);
        if (status == DomainTaskStatus.InProgress) task.ChangeStatus(DomainTaskStatus.InProgress, DateTime.UtcNow);
        else if (status == DomainTaskStatus.Done) task.Complete();
        return task;
    }

    [Fact]
    public async Task Should_ReturnOpenTasksOnly_When_NoFilterProvided()
    {
        // Arrange
        _repository.Tasks.Add(CreateTask("Pending task", DomainTaskStatus.Pending));
        _repository.Tasks.Add(CreateTask("InProgress task", DomainTaskStatus.InProgress));
        _repository.Tasks.Add(CreateTask("Done task", DomainTaskStatus.Done));

        // Act
        var result = await _sut.Handle(new ListTasksQuery());

        // Assert
        Assert.Equal(2, result.Tasks.Count);
        Assert.DoesNotContain(result.Tasks, t => t.Status == "Done");
    }

    [Fact]
    public async Task Should_ReturnAllTasks_When_StatusFilterIsAll()
    {
        // Arrange
        _repository.Tasks.Add(CreateTask("Pending task", DomainTaskStatus.Pending));
        _repository.Tasks.Add(CreateTask("InProgress task", DomainTaskStatus.InProgress));
        _repository.Tasks.Add(CreateTask("Done task", DomainTaskStatus.Done));

        // Act
        var result = await _sut.Handle(new ListTasksQuery(StatusFilter: TaskStatusFilter.All));

        // Assert
        Assert.Equal(3, result.Tasks.Count);
    }

    [Fact]
    public async Task Should_ReturnOnlyDoneTasks_When_StatusFilterIsDone()
    {
        // Arrange
        _repository.Tasks.Add(CreateTask("Pending task", DomainTaskStatus.Pending));
        _repository.Tasks.Add(CreateTask("Done task 1", DomainTaskStatus.Done));
        _repository.Tasks.Add(CreateTask("Done task 2", DomainTaskStatus.Done));

        // Act
        var result = await _sut.Handle(new ListTasksQuery(StatusFilter: TaskStatusFilter.Done));

        // Assert
        Assert.Equal(2, result.Tasks.Count);
        Assert.All(result.Tasks, t => Assert.Equal("Done", t.Status));
    }

    [Fact]
    public async Task Should_ReturnMatchingTasks_When_SearchTermProvided()
    {
        // Arrange
        _repository.Tasks.Add(CreateTask("Fix authentication bug"));
        _repository.Tasks.Add(CreateTask("Write unit tests"));
        _repository.Tasks.Add(CreateTask("Auth middleware refactor"));

        // Act
        var result = await _sut.Handle(new ListTasksQuery(SearchTerm: "auth", StatusFilter: TaskStatusFilter.All));

        // Assert
        Assert.Equal(2, result.Tasks.Count);
        Assert.All(result.Tasks, t => Assert.Contains("auth", t.Title, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Should_ReturnEmpty_When_NoTaskMatchesSearchTerm()
    {
        // Arrange
        _repository.Tasks.Add(CreateTask("Write documentation"));
        _repository.Tasks.Add(CreateTask("Deploy to production"));

        // Act
        var result = await _sut.Handle(new ListTasksQuery(SearchTerm: "xyz", StatusFilter: TaskStatusFilter.All));

        // Assert
        Assert.Empty(result.Tasks);
    }

    [Fact]
    public async Task Should_CombineSearchAndStatusFilter()
    {
        // Arrange
        _repository.Tasks.Add(CreateTask("Fix auth bug", DomainTaskStatus.Pending));
        _repository.Tasks.Add(CreateTask("Auth feature", DomainTaskStatus.Done));
        _repository.Tasks.Add(CreateTask("Deploy app", DomainTaskStatus.Done));

        // Act
        var result = await _sut.Handle(new ListTasksQuery(SearchTerm: "auth", StatusFilter: TaskStatusFilter.Done));

        // Assert
        Assert.Single(result.Tasks);
        Assert.Equal("Auth feature", result.Tasks[0].Title);
    }

    [Fact]
    public async Task Should_ReturnTasksOrderedByCreatedAtDescending()
    {
        // Arrange
        var now = DateTime.UtcNow;
        _repository.Tasks.Add(CreateTask("Oldest", createdAt: now.AddDays(-2)));
        _repository.Tasks.Add(CreateTask("Newest", createdAt: now));
        _repository.Tasks.Add(CreateTask("Middle", createdAt: now.AddDays(-1)));

        // Act
        var result = await _sut.Handle(new ListTasksQuery(StatusFilter: TaskStatusFilter.All));

        // Assert
        Assert.Equal(3, result.Tasks.Count);
        Assert.Equal("Newest", result.Tasks[0].Title);
        Assert.Equal("Middle", result.Tasks[1].Title);
        Assert.Equal("Oldest", result.Tasks[2].Title);
    }
}
