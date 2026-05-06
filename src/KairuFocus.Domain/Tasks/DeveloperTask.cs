using KairuFocus.Domain.Common;
using KairuFocus.Domain.Identity;

namespace KairuFocus.Domain.Tasks;

public sealed class DeveloperTask : AggregateRoot<TaskId>
{
    public const int MaxTags = 5;

    private readonly List<string> _tagValues = [];

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

    public IReadOnlyList<TaskTag> Tags =>
        _tagValues
            .Select(v => TaskTag.Create(v))
            .Where(r => r.IsSuccess)
            .Select(r => r.Value)
            .ToList()
            .AsReadOnly();

    // Public for EF Core materialization — domain consumers use Tags
    public List<string> TagValues
    {
        get => _tagValues;
        init => _tagValues = value;
    }

    public static DeveloperTask Create(TaskTitle title, TaskDescription? description, DateTime createdAt, UserId ownerId, IEnumerable<TaskTag>? tags = null)
    {
        var task = new DeveloperTask(TaskId.New(), ownerId, title, description, createdAt);
        if (tags is not null)
            task._tagValues.AddRange(tags.Select(t => t.Value));
        return task;
    }

    public Result Complete()
    {
        if (Status == TaskStatus.Done)
            return Result.Failure(DomainErrors.Tasks.AlreadyCompleted);

        Status = TaskStatus.Done;
        CompletedAt = DateTime.UtcNow;
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

    public Result SetTags(IReadOnlyList<TaskTag> tags)
    {
        if (tags.Count > MaxTags)
            return Result.Failure(DomainErrors.Tasks.TooManyTags);

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var tag in tags)
        {
            if (!seen.Add(tag.Value))
                return Result.Failure(DomainErrors.Tasks.DuplicateTag);
        }

        _tagValues.Clear();
        _tagValues.AddRange(tags.Select(t => t.Value));
        return Result.Success();
    }

    public void LinkJiraTicket(JiraTicketKey key) => JiraTicketKey = key;

    public void UnlinkJiraTicket() => JiraTicketKey = null;
}


