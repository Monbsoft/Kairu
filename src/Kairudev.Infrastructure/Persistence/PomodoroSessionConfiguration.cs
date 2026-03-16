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
            .HasColumnType("uniqueidentifier")
            .ValueGeneratedNever();

        builder.Property(s => s.OwnerId)
            .HasConversion(v => v.Value, v => UserId.From(v))
            .HasColumnType("nvarchar(50)")
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(s => s.SessionType)
            .HasConversion<string>()
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(s => s.PlannedDurationMinutes).HasColumnType("int").IsRequired();
        builder.Property(s => s.StartedAt).HasColumnType("datetime2");
        builder.Property(s => s.EndedAt).HasColumnType("datetime2");

        // Ignore the domain-facing property (TaskId is not an EF entity)
        builder.Ignore(s => s.LinkedTaskIds);

        // Stores the linked task IDs as a JSON array of GUIDs (EF Core 8+ primitive collection)
        builder.PrimitiveCollection(s => s.LinkedTaskIdValues)
            .HasColumnName("LinkedTaskIds")
            .HasColumnType("nvarchar(max)");
    }
}
