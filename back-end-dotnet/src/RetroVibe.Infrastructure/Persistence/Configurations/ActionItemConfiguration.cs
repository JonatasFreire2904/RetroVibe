using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetroVibe.Domain.Entities;

namespace RetroVibe.Infrastructure.Persistence.Configurations;

public sealed class ActionItemConfiguration : IEntityTypeConfiguration<ActionItem>
{
    public void Configure(EntityTypeBuilder<ActionItem> builder)
    {
        builder.ToTable("action_items");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.SessionId).HasColumnName("session_id").IsRequired();
        builder.Property(a => a.Description).HasColumnName("description").IsRequired();
        builder.Property(a => a.Status).HasColumnName("status").HasConversion<string>().IsRequired();
        builder.Property(a => a.AssigneeId).HasColumnName("assignee_id");
        builder.Property(a => a.DueDate).HasColumnName("due_date");
        builder.Property(a => a.CommentsCount).HasColumnName("comments_count");
        builder.Property(a => a.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(a => a.SessionId);
    }
}
