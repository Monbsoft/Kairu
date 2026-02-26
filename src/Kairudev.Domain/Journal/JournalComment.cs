using Kairudev.Domain.Common;

namespace Kairudev.Domain.Journal;

public sealed class JournalComment : Entity<JournalCommentId>
{
    private JournalComment(JournalCommentId id, string text) : base(id)
    {
        Text = text;
    }

    public string Text { get; private set; }

    public static Result<JournalComment> Create(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Result.Failure<JournalComment>(DomainErrors.Journal.EmptyComment);
        if (text.Length > 1000)
            return Result.Failure<JournalComment>(DomainErrors.Journal.CommentTooLong);
        return Result.Success(new JournalComment(JournalCommentId.New(), text));
    }

    public Result Update(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Result.Failure(DomainErrors.Journal.EmptyComment);
        if (text.Length > 1000)
            return Result.Failure(DomainErrors.Journal.CommentTooLong);
        Text = text;
        return Result.Success();
    }
}
