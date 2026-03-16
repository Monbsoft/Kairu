using Kairudev.Domain.Identity;
using Kairudev.Domain.Journal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kairudev.Infrastructure.Persistence.Configurations;

public sealed class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, value => JournalEntryId.From(value))
            .HasColumnType("uniqueidentifier");

        builder.Property(e => e.OwnerId)
            .HasConversion(v => v.Value, v => UserId.From(v))
            .HasColumnType("nvarchar(50)")
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(e => e.OccurredAt).HasColumnType("datetime2").IsRequired();

        builder.Property(e => e.EventType)
            .HasConversion<string>()
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(e => e.ResourceId).HasColumnType("uniqueidentifier").IsRequired();

        builder.Property(e => e.Sequence).HasColumnType("int").IsRequired(false);

        builder.HasMany(e => e.Comments)
            .WithOne()
            .HasForeignKey("EntryId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(e => e.Comments)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_comments");
    }
}
