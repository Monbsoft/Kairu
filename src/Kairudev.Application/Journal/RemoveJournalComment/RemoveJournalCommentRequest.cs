namespace Kairudev.Application.Journal.RemoveJournalComment;

public sealed record RemoveJournalCommentRequest(Guid EntryId, Guid CommentId);
