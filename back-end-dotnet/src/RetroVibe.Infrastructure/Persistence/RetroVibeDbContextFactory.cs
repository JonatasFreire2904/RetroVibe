using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RetroVibe.Infrastructure.Persistence;

public sealed class RetroVibeDbContextFactory : IDesignTimeDbContextFactory<RetroVibeDbContext>
{
    public RetroVibeDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RetroVibeDbContext>();
        optionsBuilder.UseSqlite("Data Source=design-time.db");
        return new RetroVibeDbContext(optionsBuilder.Options);
    }
}
