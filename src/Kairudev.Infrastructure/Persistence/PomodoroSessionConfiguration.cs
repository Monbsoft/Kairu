using Kairudev.Domain.Identity;
using Kairudev.Domain.Pomodoro;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kairudev.Infrastructure.Persistence;

internal sealed class PomodoroSessionConfiguration : IEntityTypeConfiguration<PomodoroSession>
{
    public void Configure(EntityTypeBuilder<PomodoroSession> builder)
    {
        builder.ToTable("PomodoroSessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasConversion(
                id => id.Value,
                value => PomodoroSessionId.From(value))
            .ValueGeneratedNever();

        builder.Property(s => s.OwnerId)
            .HasConversion(v => v.Value, v => UserId.From(v))
            .HasMaxLength(50)
            .IsRequired(false); // nullable for migration compatibility

        builder.Property(s => s.SessionType)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(s => s.PlannedDurationMinutes).IsRequired();
        builder.Property(s => s.StartedAt);
        builder.Property(s => s.EndedAt);

        // Ignore the domain-facing property (TaskId is not an EF entity)
        builder.Ignore(s => s.LinkedTaskIds);

        // Stores the linked task IDs as a JSON array of GUIDs (EF Core 8+ primitive collection)
        builder.PrimitiveCollection(s => s.LinkedTaskIdValues)
            .HasColumnName("LinkedTaskIds");
    }
}
