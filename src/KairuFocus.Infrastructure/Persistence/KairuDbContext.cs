using KairuFocus.Domain.Gamification;
using KairuFocus.Domain.Identity;
using KairuFocus.Domain.Journal;
using KairuFocus.Domain.Pomodoro;
using KairuFocus.Domain.Settings;
using KairuFocus.Domain.Tasks;
using KairuFocus.Infrastructure.Persistence.Configurations;
using KairuFocus.Infrastructure.Persistence.Internal;
using Microsoft.EntityFrameworkCore;

namespace KairuFocus.Infrastructure.Persistence;

public sealed class KairuFocusDbContext : DbContext
{
    public KairuFocusDbContext(DbContextOptions<KairuFocusDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<DeveloperTask> Tasks => Set<DeveloperTask>();
    public DbSet<PomodoroSession> PomodoroSessions => Set<PomodoroSession>();
    internal DbSet<PomodoroSettingsRow> PomodoroSettings => Set<PomodoroSettingsRow>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalComment> JournalComments => Set<JournalComment>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();
    public DbSet<McpToken> McpTokens => Set<McpToken>();
    public DbSet<XpGain> XpGains => Set<XpGain>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(string) && property.GetColumnType() == null)
                {
                    property.SetColumnType("nvarchar(max)");
                }
            }
        }

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new TaskConfiguration());
        modelBuilder.ApplyConfiguration(new PomodoroSessionConfiguration());
        modelBuilder.ApplyConfiguration(new PomodoroSettingsConfiguration());
        modelBuilder.ApplyConfiguration(new JournalEntryConfiguration());
        modelBuilder.ApplyConfiguration(new JournalCommentConfiguration());
        modelBuilder.ApplyConfiguration(new UserSettingsConfiguration());
        modelBuilder.ApplyConfiguration(new McpTokenConfiguration());
        modelBuilder.ApplyConfiguration(new XpGainConfiguration());
    }
}
