namespace Kairudev.Domain.Journal;

public sealed record JournalCommentId(Guid Value)
{
    public static JournalCommentId New() => new(Guid.NewGuid());
    public static JournalCommentId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
