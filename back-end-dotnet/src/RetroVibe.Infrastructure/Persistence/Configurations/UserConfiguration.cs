using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetroVibe.Domain.Entities;

namespace RetroVibe.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");
        builder.Property(u => u.Name).HasColumnName("name").IsRequired();
        builder.Property(u => u.Role).HasColumnName("role").IsRequired();
        builder.Property(u => u.SquadId).HasColumnName("squad_id");
        builder.Property(u => u.AvatarColor).HasColumnName("avatar_color").IsRequired();
        builder.Property(u => u.Username).HasColumnName("username");
        builder.Property(u => u.PasswordHash).HasColumnName("password_hash");
        builder.Property(u => u.AccessLevel).HasColumnName("access_level").HasConversion<string>().IsRequired();
        builder.Property(u => u.AllowedSessionId).HasColumnName("allowed_session_id");

        builder.HasIndex(u => u.Username).IsUnique();
    }
}
