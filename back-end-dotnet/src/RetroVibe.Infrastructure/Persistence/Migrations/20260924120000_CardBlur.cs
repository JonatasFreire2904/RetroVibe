using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RetroVibe.Infrastructure.Persistence.Migrations;

[DbContext(typeof(RetroVibeDbContext))]
[Migration("20260924120000_CardBlur")]
public sealed class CardBlur : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>("card_blur_enabled", "sessions", type: "INTEGER", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<bool>("cards_revealed", "sessions", type: "INTEGER", nullable: false, defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("card_blur_enabled", "sessions");
        migrationBuilder.DropColumn("cards_revealed", "sessions");
    }
}
