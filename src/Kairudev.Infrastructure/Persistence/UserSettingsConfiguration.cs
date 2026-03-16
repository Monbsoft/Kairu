using Kairudev.Domain.Identity;
using Kairudev.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kairudev.Infrastructure.Persistence;

internal sealed class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.ToTable("UserSettings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasConversion(v => v.Value, v => UserId.From(v))
            .HasColumnType("nvarchar(50)")
            .HasMaxLength(50)
            .ValueGeneratedNever();

        builder.Property(s => s.ThemePreference)
            .HasConversion<string>()
            .HasColumnType("nvarchar(20)")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.RingtonePreference)
            .HasConversion<string>()
            .HasColumnType("nvarchar(20)")
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(RingtonePreference.AlarmClock);

        builder.Property(s => s.JiraBaseUrl).HasColumnType("nvarchar(500)").HasMaxLength(500);
        builder.Property(s => s.JiraEmail).HasColumnType("nvarchar(200)").HasMaxLength(200);
        builder.Property(s => s.JiraApiToken).HasColumnType("nvarchar(500)").HasMaxLength(500);
    }
}
