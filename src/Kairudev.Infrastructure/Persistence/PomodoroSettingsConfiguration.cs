using Kairudev.Infrastructure.Persistence.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kairudev.Infrastructure.Persistence;

internal sealed class PomodoroSettingsConfiguration : IEntityTypeConfiguration<PomodoroSettingsRow>
{
    public void Configure(EntityTypeBuilder<PomodoroSettingsRow> builder)
    {
        builder.ToTable("PomodoroSettings");
        builder.HasKey(s => s.UserId);
        builder.Property(s => s.UserId).HasColumnType("nvarchar(50)").HasMaxLength(50).ValueGeneratedNever();
        builder.Property(s => s.SprintDurationMinutes).HasColumnType("int").IsRequired();
        builder.Property(s => s.ShortBreakDurationMinutes).HasColumnType("int").IsRequired();
        builder.Property(s => s.LongBreakDurationMinutes).HasColumnType("int").IsRequired();
    }
}
