using Kairudev.Domain.Journal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kairudev.Infrastructure.Persistence.Configurations;

public sealed class JournalCommentConfiguration : IEntityTypeConfiguration<JournalComment>
{
    public void Configure(EntityTypeBuilder<JournalComment> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => JournalCommentId.From(value))
            .HasColumnType("uniqueidentifier");

        builder.Property(c => c.Text).HasColumnType("nvarchar(1000)").IsRequired().HasMaxLength(1000);
    }
}
