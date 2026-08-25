using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetroVibe.Infrastructure.Persistence.Rows;

namespace RetroVibe.Infrastructure.Persistence.Configurations;

public sealed class SquadRowConfiguration : IEntityTypeConfiguration<SquadRow>
{
    public void Configure(EntityTypeBuilder<SquadRow> builder)
    {
        builder.ToTable("squads");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.Name).HasColumnName("name").IsRequired();
        builder.HasIndex(s => s.Name).IsUnique();
    }
}

public sealed class ThemeRowConfiguration : IEntityTypeConfiguration<ThemeRow>
{
    public void Configure(EntityTypeBuilder<ThemeRow> builder)
    {
        builder.ToTable("themes");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.Key).HasColumnName("key").IsRequired();
        builder.Property(t => t.Label).HasColumnName("label").IsRequired();
        builder.Property(t => t.Emoji).HasColumnName("emoji").IsRequired();
        builder.Property(t => t.Active).HasColumnName("active");
        builder.HasIndex(t => t.Key).IsUnique();
    }
}

public sealed class TemplateRowConfiguration : IEntityTypeConfiguration<TemplateRow>
{
    public void Configure(EntityTypeBuilder<TemplateRow> builder)
    {
        builder.ToTable("templates");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.Key).HasColumnName("key").IsRequired();
        builder.Property(t => t.Label).HasColumnName("label").IsRequired();
        builder.Property(t => t.Icon).HasColumnName("icon").IsRequired();
        builder.Property(t => t.Description).HasColumnName("description").IsRequired();
        builder.Property(t => t.IsCustom).HasColumnName("is_custom");
        builder.Property(t => t.Active).HasColumnName("active");
        builder.HasIndex(t => t.Key).IsUnique();

        builder.HasMany(t => t.Columns)
            .WithOne()
            .HasForeignKey(c => c.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class TemplateColumnRowConfiguration : IEntityTypeConfiguration<TemplateColumnRow>
{
    public void Configure(EntityTypeBuilder<TemplateColumnRow> builder)
    {
        builder.ToTable("template_columns");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.TemplateId).HasColumnName("template_id").IsRequired();
        builder.Property(c => c.Key).HasColumnName("key").IsRequired();
        builder.Property(c => c.Label).HasColumnName("label").IsRequired();
        builder.Property(c => c.Icon).HasColumnName("icon").IsRequired();
        builder.Property(c => c.OrderIndex).HasColumnName("order_index");
    }
}
