namespace Kairudev.Application.Journal.UpdateJournalComment;

public sealed record UpdateJournalCommentRequest(Guid EntryId, Guid CommentId, string Text);
