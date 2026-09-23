using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RetroVibe.Infrastructure.Persistence.Migrations;

[DbContext(typeof(RetroVibeDbContext))]
[Migration("20260923150000_SessionConfiguration")]
public sealed class SessionConfiguration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>("action_cards_enabled", "sessions", type: "INTEGER", nullable: false, defaultValue: true);
        migrationBuilder.AddColumn<DateTime>("stage_started_at", "sessions", type: "TEXT", nullable: false, defaultValue: DateTime.UnixEpoch);
        migrationBuilder.Sql("UPDATE sessions SET stage_started_at = CURRENT_TIMESTAMP");
        migrationBuilder.AddColumn<int>("collect_seconds", "sessions", type: "INTEGER", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<int>("vote_seconds", "sessions", type: "INTEGER", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<int>("discuss_seconds", "sessions", type: "INTEGER", nullable: false, defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("action_cards_enabled", "sessions");
        migrationBuilder.DropColumn("stage_started_at", "sessions");
        migrationBuilder.DropColumn("collect_seconds", "sessions");
        migrationBuilder.DropColumn("vote_seconds", "sessions");
        migrationBuilder.DropColumn("discuss_seconds", "sessions");
    }
}
