namespace Kairudev.Domain.Journal;

public static class DomainErrors
{
    public static class Journal
    {
        public const string EmptyComment    = "Comment cannot be empty.";
        public const string CommentTooLong  = "Comment cannot exceed 1000 characters.";
        public const string CommentNotFound = "Comment not found.";
        public const string EntryNotFound   = "Journal entry not found.";
    }
}
