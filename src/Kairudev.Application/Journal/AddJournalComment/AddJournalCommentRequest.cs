namespace Kairudev.Application.Journal.AddJournalComment;

public sealed record AddJournalCommentRequest(Guid EntryId, string Text);
