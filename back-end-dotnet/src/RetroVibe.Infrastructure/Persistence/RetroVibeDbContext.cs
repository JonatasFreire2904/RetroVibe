using Microsoft.EntityFrameworkCore;
using RetroVibe.Domain.Entities;
using RetroVibe.Infrastructure.Persistence.Rows;

namespace RetroVibe.Infrastructure.Persistence;

public sealed class RetroVibeDbContext : DbContext
{
    public RetroVibeDbContext(DbContextOptions<RetroVibeDbContext> options) : base(options) { }

    public DbSet<SquadRow> Squads => Set<SquadRow>();
    public DbSet<ThemeRow> Themes => Set<ThemeRow>();
    public DbSet<TemplateRow> Templates => Set<TemplateRow>();
    public DbSet<TemplateColumnRow> TemplateColumns => Set<TemplateColumnRow>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RetroSession> Sessions => Set<RetroSession>();
    public DbSet<ActionItem> ActionItems => Set<ActionItem>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RetroVibeDbContext).Assembly);
    }
}
