using Kairu.Domain.Identity;
using Kairu.Domain.Journal;
using Kairu.Domain.Pomodoro;
using Kairu.Domain.Settings;
using Kairu.Domain.Tasks;
using Kairu.Infrastructure.Persistence.Configurations;
using Kairu.Infrastructure.Persistence.Internal;
using Microsoft.EntityFrameworkCore;

namespace Kairu.Infrastructure.Persistence;

public sealed class KairuDbContext : DbContext
{
    public KairuDbContext(DbContextOptions<KairuDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<DeveloperTask> Tasks => Set<DeveloperTask>();
    public DbSet<PomodoroSession> PomodoroSessions => Set<PomodoroSession>();
    internal DbSet<PomodoroSettingsRow> PomodoroSettings => Set<PomodoroSettingsRow>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalComment> JournalComments => Set<JournalComment>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();

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
    }
}
