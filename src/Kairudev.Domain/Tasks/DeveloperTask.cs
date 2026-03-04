using Kairudev.Domain.Common;
using Kairudev.Domain.Identity;

namespace Kairudev.Domain.Tasks;

public sealed class DeveloperTask : AggregateRoot<TaskId>
{
    private DeveloperTask(TaskId id, UserId ownerId, TaskTitle title, TaskDescription? description, DateTime createdAt)
        : base(id)
    {
        OwnerId = ownerId;
        Title = title;
        Description = description;
        Status = TaskStatus.Pending;
        CreatedAt = createdAt;
    }

    public UserId OwnerId { get; private set; } = default!;
    public TaskTitle Title { get; private set; }
    public TaskDescription? Description { get; private set; }
    public TaskStatus Status { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? CompletedAt { get; private set; }
    public JiraTicketKey? JiraTicketKey { get; private set; }

    public static DeveloperTask Create(TaskTitle title, TaskDescription? description, DateTime createdAt, UserId ownerId) =>
        new(TaskId.New(), ownerId, title, description, createdAt);

    public Result Complete()
    {
        if (Status == TaskStatus.Done)
            return Result.Failure(DomainErrors.Tasks.AlreadyCompleted);

        Status = TaskStatus.Done;
        CompletedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result StartProgress()
    {
        if (Status == TaskStatus.Done)
            return Result.Failure(DomainErrors.Tasks.AlreadyCompleted);

        Status = TaskStatus.InProgress;
        return Result.Success();
    }

    public Result ChangeStatus(TaskStatus newStatus, DateTime now)
    {
        if (Status == newStatus)
            return Result.Failure(DomainErrors.Tasks.SameStatus);

        if (newStatus == TaskStatus.Done)
            CompletedAt = now;
        else if (Status == TaskStatus.Done)
            CompletedAt = null;

        Status = newStatus;
        return Result.Success();
    }

    public void UpdateDetails(TaskTitle title, TaskDescription? description)
    {
        Title = title;
        Description = description;
    }

    public void LinkJiraTicket(JiraTicketKey key) => JiraTicketKey = key;

    public void UnlinkJiraTicket() => JiraTicketKey = null;
}
