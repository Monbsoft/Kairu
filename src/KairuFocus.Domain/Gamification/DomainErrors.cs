namespace KairuFocus.Domain.Gamification;

// Suggested alias at call sites to avoid conflicts with other bounded contexts:
// using GamificationErrors = KairuFocus.Domain.Gamification.DomainErrors;
public static class DomainErrors
{
    public static class Gamification
    {
        public const string SessionNotFound = "Session not found for XP credit.";
    }
}
