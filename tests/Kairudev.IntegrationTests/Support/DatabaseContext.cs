using Kairudev.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Kairudev.IntegrationTests.Support;

public class DatabaseContext : IDisposable
{
    private readonly DbContextOptions<KairudevDbContext> _options;
    public KairudevDbContext DbContext { get; private set; }

    public DatabaseContext()
    {
        // Use shared in-memory SQLite for testing - allows multiple connections to same DB
        var builder = new DbContextOptionsBuilder<KairudevDbContext>()
            .UseSqlite("Data Source=file:memdb?mode=memory&cache=shared;");

        _options = builder.Options;

        DbContext = new KairudevDbContext(_options);
        // Enable foreign keys (disabled by default in SQLite)
        DbContext.Database.ExecuteSqlRaw("PRAGMA foreign_keys = ON;");
        // Create tables directly from model without migrations
        EnsureTablesCreated();
    }

    private void EnsureTablesCreated()
    {
        // For SQLite in-memory, bypass migrations entirely and create schema directly
        var connection = DbContext.Database.GetDbConnection();
        connection.Open();

        try
        {
            using (var command = connection.CreateCommand())
            {
                // Create all tables using raw SQL (SQLite-compatible types only)
                // Note: ForeignKeys are NOT enforced in integration tests to allow flexible test setup
                var sql = @"
CREATE TABLE IF NOT EXISTS ""Users"" (
    ""Id"" TEXT NOT NULL PRIMARY KEY,
    ""GitHubId"" TEXT NOT NULL UNIQUE,
    ""Login"" TEXT NOT NULL,
    ""DisplayName"" TEXT NOT NULL,
    ""Email"" TEXT
);

CREATE TABLE IF NOT EXISTS ""Tasks"" (
    ""Id"" TEXT NOT NULL PRIMARY KEY,
    ""OwnerId"" TEXT,
    ""Title"" TEXT NOT NULL,
    ""Description"" TEXT,
    ""CreatedAt"" TEXT NOT NULL,
    ""CompletedAt"" TEXT,
    ""Status"" TEXT NOT NULL,
    ""JiraTicketKey"" TEXT
);

CREATE TABLE IF NOT EXISTS ""PomodoroSessions"" (
    ""Id"" TEXT NOT NULL PRIMARY KEY,
    ""OwnerId"" TEXT,
    ""SessionType"" TEXT NOT NULL,
    ""Status"" TEXT NOT NULL,
    ""PlannedDurationMinutes"" INTEGER NOT NULL,
    ""StartedAt"" TEXT,
    ""EndedAt"" TEXT,
    ""LinkedTaskIds"" TEXT
);

CREATE TABLE IF NOT EXISTS ""PomodoroSettings"" (
    ""UserId"" TEXT NOT NULL PRIMARY KEY,
    ""SprintDurationMinutes"" INTEGER NOT NULL,
    ""ShortBreakDurationMinutes"" INTEGER NOT NULL,
    ""LongBreakDurationMinutes"" INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS ""JournalEntries"" (
    ""Id"" TEXT NOT NULL PRIMARY KEY,
    ""OwnerId"" TEXT,
    ""OccurredAt"" TEXT NOT NULL,
    ""EventType"" TEXT NOT NULL,
    ""ResourceId"" TEXT NOT NULL,
    ""Sequence"" INTEGER
);

CREATE TABLE IF NOT EXISTS ""JournalComments"" (
    ""Id"" TEXT NOT NULL PRIMARY KEY,
    ""EntryId"" TEXT NOT NULL,
    ""Text"" TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS ""UserSettings"" (
    ""Id"" TEXT NOT NULL PRIMARY KEY,
    ""ThemePreference"" TEXT NOT NULL,
    ""RingtonePreference"" TEXT NOT NULL,
    ""JiraBaseUrl"" TEXT,
    ""JiraEmail"" TEXT,
    ""JiraApiToken"" TEXT
);
";

                command.CommandText = sql;
                command.ExecuteNonQuery();
            }
        }
        finally
        {
            connection.Close();
        }
    }

    public void Dispose()
    {
        DbContext?.Dispose();
    }

    public void Reset()
    {
        DbContext?.Dispose();
        DbContext = new KairudevDbContext(_options);

        // Delete all data
        DbContext.Database.ExecuteSqlRaw("DELETE FROM JournalComments;");
        DbContext.Database.ExecuteSqlRaw("DELETE FROM JournalEntries;");
        DbContext.Database.ExecuteSqlRaw("DELETE FROM PomodoroSessions;");
        DbContext.Database.ExecuteSqlRaw("DELETE FROM PomodoroSettings;");
        DbContext.Database.ExecuteSqlRaw("DELETE FROM Tasks;");
        DbContext.Database.ExecuteSqlRaw("DELETE FROM UserSettings;");
        DbContext.Database.ExecuteSqlRaw("DELETE FROM Users;");
    }
}
