using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RetroVibe.Infrastructure.Persistence.Migrations;

[DbContext(typeof(RetroVibeDbContext))]
[Migration("20261005160000_UpdatedSurveyQuestions")]
public sealed class UpdatedSurveyQuestions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "team_more_engaged",
            table: "survey_responses",
            type: "INTEGER",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "used_custom_theme",
            table: "survey_responses",
            type: "INTEGER",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "team_more_engaged", table: "survey_responses");
        migrationBuilder.DropColumn(name: "used_custom_theme", table: "survey_responses");
    }
}
