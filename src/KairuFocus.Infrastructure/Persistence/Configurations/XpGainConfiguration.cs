using KairuFocus.Domain.Gamification;
using KairuFocus.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KairuFocus.Infrastructure.Persistence.Configurations;

public sealed class XpGainConfiguration : IEntityTypeConfiguration<XpGain>
{
    public void Configure(EntityTypeBuilder<XpGain> builder)
    {
        builder.ToTable("XpGains");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
            .HasConversion(id => id.Value, value => XpGainId.From(value))
            .HasColumnType("uniqueidentifier")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(g => g.OwnerId)
            .HasConversion(v => v.Value, v => UserId.From(v))
            .HasColumnType("uniqueidentifier")
            .IsRequired();

        // No FK towards PomodoroSessions on purpose: the ledger is an autonomous
        // historical record and must survive a session purge (ADR-022).
        builder.Property(g => g.SessionId)
            .HasColumnType("uniqueidentifier")
            .IsRequired();

        // Idempotence: one session can be credited at most once.
        builder.HasIndex(g => g.SessionId)
            .IsUnique()
            .HasDatabaseName("IX_XpGains_SessionId");

        builder.Property(g => g.Amount)
            .HasColumnType("int")
            .IsRequired();

        builder.Property(g => g.EarnedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        // Covers total/weekly XP reads (and the weekly leaderboard aggregate in iteration B).
        builder.HasIndex(g => new { g.OwnerId, g.EarnedAtUtc })
            .HasDatabaseName("IX_XpGains_OwnerId_EarnedAtUtc");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(g => g.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
