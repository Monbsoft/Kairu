using Kairudev.Domain.Identity;
using Kairudev.Domain.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kairudev.Infrastructure.Persistence;

internal sealed class TaskConfiguration : IEntityTypeConfiguration<DeveloperTask>
{
    public void Configure(EntityTypeBuilder<DeveloperTask> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasConversion(
                id => id.Value,
                value => TaskId.From(value))
            .HasColumnType("uniqueidentifier")
            .ValueGeneratedNever();

        builder.Property(t => t.OwnerId)
            .HasConversion(v => v.Value, v => UserId.From(v))
            .HasColumnType("nvarchar(50)")
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(t => t.Title)
            .HasConversion(
                title => title.Value,
                value => TaskTitle.Create(value).Value)
            .HasColumnType("nvarchar(200)")
            .HasMaxLength(TaskTitle.MaxLength)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasConversion(
                desc => desc != null ? desc.Value : null,
                value => TaskDescription.Create(value).Value)
            .HasColumnType("nvarchar(1000)")
            .HasMaxLength(TaskDescription.MaxLength);

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(t => t.CreatedAt).HasColumnType("datetime2").IsRequired();
        builder.Property(t => t.CompletedAt).HasColumnType("datetime2");

        builder.Property(t => t.JiraTicketKey)
            .HasConversion(
                key => key != null ? key.Value : null,
                value => value != null ? JiraTicketKey.Create(value).Value : null)
            .HasColumnType("nvarchar(50)")
            .HasMaxLength(JiraTicketKey.MaxLength);
    }
}
