using Kairu.Domain.Identity;
using Kairu.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kairu.Infrastructure.Persistence;

internal sealed class UserApiKeyConfiguration : IEntityTypeConfiguration<UserApiKey>
{
    public void Configure(EntityTypeBuilder<UserApiKey> builder)
    {
        builder.ToTable("UserApiKeys");

        builder.HasKey(k => k.Id);

        builder.Property(k => k.Id)
            .HasConversion(v => v.Value, v => UserId.From(v))
            .ValueGeneratedNever();

        builder.Property(k => k.KeyHash)
            .HasColumnType("nvarchar(64)")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(k => k.CreatedAt)
            .IsRequired();

        // OwnerId is a computed property => Id, no column mapping needed
        builder.Ignore(k => k.OwnerId);
    }
}
