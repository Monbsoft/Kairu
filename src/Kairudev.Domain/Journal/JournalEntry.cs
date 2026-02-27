using Kairudev.Domain.Common;

namespace Kairudev.Domain.Journal;

public sealed class JournalEntry : AggregateRoot<JournalEntryId>
{
    private readonly List<JournalComment> _comments = [];

    private JournalEntry(JournalEntryId id, DateTime occurredAt, JournalEventType eventType, Guid resourceId, int? sequence)
        : base(id)
    {
        OccurredAt = occurredAt;
        EventType = eventType;
        ResourceId = resourceId;
        Sequence = sequence;
    }

    public DateTime OccurredAt { get; }
    public JournalEventType EventType { get; }
    public Guid ResourceId { get; }
    public int? Sequence { get; }
    public IReadOnlyList<JournalComment> Comments => _comments.AsReadOnly();

    public static JournalEntry Create(JournalEventType eventType, Guid resourceId, DateTime occurredAt, int? sequence = null)
        => new(JournalEntryId.New(), occurredAt, eventType, resourceId, sequence);

    public Result<JournalCommentId> AddComment(string text)
    {
        var result = JournalComment.Create(text);
        if (result.IsFailure)
            return Result.Failure<JournalCommentId>(result.Error);
        _comments.Add(result.Value);
        return Result.Success(result.Value.Id);
    }

    public Result UpdateComment(JournalCommentId commentId, string text)
    {
        var comment = _comments.FirstOrDefault(c => c.Id == commentId);
        if (comment is null)
            return Result.Failure(DomainErrors.Journal.CommentNotFound);
        return comment.Update(text);
    }

    public Result RemoveComment(JournalCommentId commentId)
    {
        var comment = _comments.FirstOrDefault(c => c.Id == commentId);
        if (comment is null)
            return Result.Failure(DomainErrors.Journal.CommentNotFound);
        _comments.Remove(comment);
        return Result.Success();
    }
}
