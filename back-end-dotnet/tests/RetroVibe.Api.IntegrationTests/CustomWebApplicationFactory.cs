using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RetroVibe.Infrastructure.Persistence;

namespace RetroVibe.Api.IntegrationTests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    private readonly IReadOnlyDictionary<string, string?>? _settings;

    public CustomWebApplicationFactory(IReadOnlyDictionary<string, string?>? settings = null)
    {
        _settings = settings;
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        if (_settings is not null)
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(_settings));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<RetroVibeDbContext>>();
            services.AddDbContext<RetroVibeDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }
}
