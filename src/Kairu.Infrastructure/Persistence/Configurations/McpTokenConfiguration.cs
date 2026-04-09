using Kairu.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kairu.Infrastructure.Persistence.Configurations;

public sealed class McpTokenConfiguration : IEntityTypeConfiguration<McpToken>
{
    public void Configure(EntityTypeBuilder<McpToken> builder)
    {
        builder.ToTable("McpTokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, value => McpTokenId.From(value))
            .HasColumnType("uniqueidentifier")
            .IsRequired();

        builder.Property(t => t.UserId)
            .HasConversion(v => v.Value, v => UserId.From(v))
            .HasColumnType("uniqueidentifier")
            .IsRequired();

        builder.HasIndex(t => t.UserId)
            .IsUnique()
            .HasDatabaseName("IX_McpTokens_UserId");

        builder.Property(t => t.TokenHash)
            .HasConversion(h => h.Value, v => McpTokenHash.Create(v).Value!)
            .HasColumnType("nvarchar(64)")
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(t => t.TokenHash)
            .IsUnique()
            .HasDatabaseName("IX_McpTokens_TokenHash");

        builder.Property(t => t.ExpiresAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();
    }
}
