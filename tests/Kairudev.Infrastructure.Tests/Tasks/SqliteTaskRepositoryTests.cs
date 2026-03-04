using Kairudev.Domain.Identity;
using Kairudev.Domain.Tasks;
using Kairudev.Infrastructure.Persistence;
using DomainTaskStatus = Kairudev.Domain.Tasks.TaskStatus;

namespace Kairudev.Infrastructure.Tests.Tasks;

public sealed class SqliteTaskRepositoryTests : InfrastructureTestBase
{
    private static readonly UserId OwnerId = UserId.From("test-user");

    private readonly SqliteTaskRepository _repository;

    public SqliteTaskRepositoryTests()
    {
        _repository = new SqliteTaskRepository(Context);
    }

    [Fact]
    public async Task Should_PersistTask_When_Added()
    {
        var title = TaskTitle.Create("Test task").Value;
        var task = DeveloperTask.Create(title, null, DateTime.UtcNow, OwnerId);

        await _repository.AddAsync(task);

        var stored = await _repository.GetByIdAsync(task.Id, OwnerId);
        Assert.NotNull(stored);
        Assert.Equal(task.Id, stored.Id);
        Assert.Equal("Test task", stored.Title.Value);
        Assert.Equal(DomainTaskStatus.Pending, stored.Status);
    }

    [Fact]
    public async Task Should_ReturnNull_When_TaskNotFound()
    {
        var result = await _repository.GetByIdAsync(TaskId.New(), OwnerId);

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_ReturnAllTasks_When_MultipleAdded()
    {
        var t1 = DeveloperTask.Create(TaskTitle.Create("First").Value, null, DateTime.UtcNow, OwnerId);
        var t2 = DeveloperTask.Create(TaskTitle.Create("Second").Value, null, DateTime.UtcNow.AddSeconds(1), OwnerId);

        await _repository.AddAsync(t1);
        await _repository.AddAsync(t2);

        var all = await _repository.GetAllAsync(OwnerId);
        Assert.Equal(2, all.Count);
        Assert.Equal("First", all[0].Title.Value);
        Assert.Equal("Second", all[1].Title.Value);
    }

    [Fact]
    public async Task Should_ReturnEmptyList_When_NoTasks()
    {
        var all = await _repository.GetAllAsync(OwnerId);

        Assert.Empty(all);
    }

    [Fact]
    public async Task Should_PersistStatusChange_When_TaskUpdated()
    {
        var task = DeveloperTask.Create(TaskTitle.Create("Updatable task").Value, null, DateTime.UtcNow, OwnerId);
        await _repository.AddAsync(task);

        task.Complete();
        await _repository.UpdateAsync(task);

        var stored = await _repository.GetByIdAsync(task.Id, OwnerId);
        Assert.NotNull(stored);
        Assert.Equal(DomainTaskStatus.Done, stored.Status);
        Assert.NotNull(stored.CompletedAt);
    }

    [Fact]
    public async Task Should_RemoveTask_When_Deleted()
    {
        var task = DeveloperTask.Create(TaskTitle.Create("To delete").Value, null, DateTime.UtcNow, OwnerId);
        await _repository.AddAsync(task);

        await _repository.DeleteAsync(task.Id, OwnerId);

        var stored = await _repository.GetByIdAsync(task.Id, OwnerId);
        Assert.Null(stored);
    }
}
