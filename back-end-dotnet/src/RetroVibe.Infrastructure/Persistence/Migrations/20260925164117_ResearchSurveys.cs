using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetroVibe.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ResearchSurveys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_test",
                table: "users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "managed_squad_ids",
                table: "users",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.Sql("UPDATE users SET managed_squad_ids = json_array(squad_id) WHERE access_level = 'Facilitator' AND squad_id IS NOT NULL");

            migrationBuilder.AddColumn<int>(
                name: "token_version",
                table: "users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "facilitator_id",
                table: "sessions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_test",
                table: "sessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "survey_enabled",
                table: "sessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "survey_responses",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", nullable: false),
                    session_id = table.Column<string>(type: "TEXT", nullable: false),
                    respondent_id = table.Column<string>(type: "TEXT", nullable: false),
                    respondent_role = table.Column<string>(type: "TEXT", nullable: false),
                    engagement_score = table.Column<int>(type: "INTEGER", nullable: false),
                    usability_score = table.Column<int>(type: "INTEGER", nullable: false),
                    suggestion = table.Column<string>(type: "TEXT", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_survey_responses", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_survey_responses_session_id_respondent_id",
                table: "survey_responses",
                columns: new[] { "session_id", "respondent_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "survey_responses");

            migrationBuilder.DropColumn(
                name: "is_test",
                table: "users");

            migrationBuilder.DropColumn(
                name: "managed_squad_ids",
                table: "users");

            migrationBuilder.DropColumn(
                name: "token_version",
                table: "users");

            migrationBuilder.DropColumn(
                name: "facilitator_id",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "is_test",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "survey_enabled",
                table: "sessions");
        }
    }
}
