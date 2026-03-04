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
            .HasMaxLength(50)
            .ValueGeneratedNever();

        builder.Property(s => s.ThemePreference)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.RingtonePreference)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(RingtonePreference.AlarmClock);

        builder.Property(s => s.JiraBaseUrl).HasMaxLength(500);
        builder.Property(s => s.JiraEmail).HasMaxLength(200);
        builder.Property(s => s.JiraApiToken).HasMaxLength(500);
    }
}
