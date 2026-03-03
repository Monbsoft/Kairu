namespace Kairudev.Domain.Tasks;

public static class DomainErrors
{
    public static class Tasks
    {
        public const string EmptyTitle = "Task title cannot be empty.";
        public const string TitleTooLong = "Task title cannot exceed 200 characters.";
        public const string DescriptionTooLong = "Task description cannot exceed 1000 characters.";
        public const string AlreadyCompleted = "Task is already completed.";
        public const string NotFound = "Task not found.";
        public const string SameStatus = "Task is already in the requested status.";
        public const string EmptyJiraTicketKey = "Jira ticket key cannot be empty.";
        public const string JiraTicketKeyTooLong = "Jira ticket key cannot exceed 50 characters.";
        public const string InvalidJiraTicketKeyFormat = "Jira ticket key must follow the format PROJECT-123.";
        public const string JiraNotConfigured = "Jira is not configured. Please set your Jira credentials in Settings.";
    }
}
