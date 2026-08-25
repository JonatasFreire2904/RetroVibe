using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetroVibe.Domain.Entities;

namespace RetroVibe.Infrastructure.Persistence.Configurations;

public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("comments");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.ParentKind).HasColumnName("parent_kind").HasConversion<string>().IsRequired();
        builder.Property(c => c.ParentId).HasColumnName("parent_id").IsRequired();
        builder.Property(c => c.AuthorId).HasColumnName("author_id").IsRequired();
        builder.Property(c => c.Text).HasColumnName("text").IsRequired();
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(c => new { c.ParentKind, c.ParentId });
    }
}
