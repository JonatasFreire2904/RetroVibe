using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RetroVibe.Infrastructure.Persistence.Migrations;

[DbContext(typeof(RetroVibeDbContext))]
[Migration("20261009180000_SusUesQuestionnaire")]
public sealed class SusUesQuestionnaire : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "questionnaire_version", table: "survey_responses", type: "INTEGER",
            nullable: false, defaultValue: 1);
        migrationBuilder.AddColumn<string>(
            name: "ratings_json", table: "survey_responses", type: "TEXT", nullable: true);
        migrationBuilder.AddColumn<string>(
            name: "custom_theme_name", table: "survey_responses", type: "TEXT", nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "questionnaire_version", table: "survey_responses");
        migrationBuilder.DropColumn(name: "ratings_json", table: "survey_responses");
        migrationBuilder.DropColumn(name: "custom_theme_name", table: "survey_responses");
    }
}
