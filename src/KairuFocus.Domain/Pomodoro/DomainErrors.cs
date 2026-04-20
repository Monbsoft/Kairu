namespace KairuFocus.Domain.Pomodoro;

public static class DomainErrors
{
    public static class Pomodoro
    {
        public const string SessionAlreadyActive = "A Pomodoro session is already active.";
        public const string SessionNotActive = "No Pomodoro session is currently active.";
        public const string NoActiveSession = "No active session.";
        public const string SessionNotFound = "Pomodoro session not found.";
        public const string TaskAlreadyLinked = "Task is already linked to this session.";
        public const string TaskNotLinked = "Task is not linked to this session.";
        public const string InvalidDuration = "Duration must be between 1 and 120 minutes.";
        public const string InvalidTransition = "Invalid status transition.";
        public const string TaskLinkingNotAllowedForBreak = "Linking a task is only allowed during a Sprint session.";
    }
}
