using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RetroVibe.Infrastructure.Persistence.Migrations;

[DbContext(typeof(RetroVibeDbContext))]
[Migration("20260922150000_SequentialRetroFlow")]
public sealed class SequentialRetroFlow : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "active_column_index", table: "sessions", type: "INTEGER", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<bool>(
            name: "sequential_flow", table: "sessions", type: "INTEGER", nullable: false, defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "active_column_index", table: "sessions");
        migrationBuilder.DropColumn(name: "sequential_flow", table: "sessions");
    }
}
