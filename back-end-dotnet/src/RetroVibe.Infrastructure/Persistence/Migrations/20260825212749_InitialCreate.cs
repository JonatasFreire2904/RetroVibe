using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetroVibe.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "action_items",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", nullable: false),
                    session_id = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: false),
                    status = table.Column<string>(type: "TEXT", nullable: false),
                    assignee_id = table.Column<string>(type: "TEXT", nullable: true),
                    due_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    comments_count = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_action_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "comments",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", nullable: false),
                    parent_kind = table.Column<string>(type: "TEXT", nullable: false),
                    parent_id = table.Column<string>(type: "TEXT", nullable: false),
                    author_id = table.Column<string>(type: "TEXT", nullable: false),
                    text = table.Column<string>(type: "TEXT", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", nullable: false),
                    title = table.Column<string>(type: "TEXT", nullable: true),
                    template_id = table.Column<string>(type: "TEXT", nullable: false),
                    theme_id = table.Column<string>(type: "TEXT", nullable: false),
                    squad_id = table.Column<string>(type: "TEXT", nullable: false),
                    status = table.Column<string>(type: "TEXT", nullable: false),
                    phase = table.Column<string>(type: "TEXT", nullable: false),
                    privacy_mode = table.Column<string>(type: "TEXT", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    closed_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    participants_count = table.Column<int>(type: "INTEGER", nullable: false),
                    feedback_score = table.Column<double>(type: "REAL", nullable: true),
                    collect_minutes = table.Column<int>(type: "INTEGER", nullable: true),
                    vote_minutes = table.Column<int>(type: "INTEGER", nullable: true),
                    discuss_minutes = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "squads",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_squads", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "templates",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", nullable: false),
                    key = table.Column<string>(type: "TEXT", nullable: false),
                    label = table.Column<string>(type: "TEXT", nullable: false),
                    icon = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: false),
                    is_custom = table.Column<bool>(type: "INTEGER", nullable: false),
                    active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "themes",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", nullable: false),
                    key = table.Column<string>(type: "TEXT", nullable: false),
                    label = table.Column<string>(type: "TEXT", nullable: false),
                    emoji = table.Column<string>(type: "TEXT", nullable: false),
                    active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_themes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    role = table.Column<string>(type: "TEXT", nullable: false),
                    squad_id = table.Column<string>(type: "TEXT", nullable: true),
                    avatar_color = table.Column<string>(type: "TEXT", nullable: false),
                    username = table.Column<string>(type: "TEXT", nullable: true),
                    password_hash = table.Column<string>(type: "TEXT", nullable: true),
                    access_level = table.Column<string>(type: "TEXT", nullable: false),
                    allowed_session_id = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "columns",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", nullable: false),
                    key = table.Column<string>(type: "TEXT", nullable: false),
                    label = table.Column<string>(type: "TEXT", nullable: false),
                    icon = table.Column<string>(type: "TEXT", nullable: false),
                    order_index = table.Column<int>(type: "INTEGER", nullable: false),
                    session_id = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_columns", x => x.id);
                    table.ForeignKey(
                        name: "FK_columns_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "template_columns",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", nullable: false),
                    template_id = table.Column<string>(type: "TEXT", nullable: false),
                    key = table.Column<string>(type: "TEXT", nullable: false),
                    label = table.Column<string>(type: "TEXT", nullable: false),
                    icon = table.Column<string>(type: "TEXT", nullable: false),
                    order_index = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_template_columns", x => x.id);
                    table.ForeignKey(
                        name: "FK_template_columns_templates_template_id",
                        column: x => x.template_id,
                        principalTable: "templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cards",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", nullable: false),
                    column_id = table.Column<string>(type: "TEXT", nullable: false),
                    author_id = table.Column<string>(type: "TEXT", nullable: false),
                    text = table.Column<string>(type: "TEXT", nullable: false),
                    comments_count = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    voter_ids = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cards", x => x.id);
                    table.ForeignKey(
                        name: "FK_cards_columns_column_id",
                        column: x => x.column_id,
                        principalTable: "columns",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_action_items_session_id",
                table: "action_items",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_cards_column_id",
                table: "cards",
                column: "column_id");

            migrationBuilder.CreateIndex(
                name: "IX_columns_session_id",
                table: "columns",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_comments_parent_kind_parent_id",
                table: "comments",
                columns: new[] { "parent_kind", "parent_id" });

            migrationBuilder.CreateIndex(
                name: "IX_sessions_squad_id",
                table: "sessions",
                column: "squad_id");

            migrationBuilder.CreateIndex(
                name: "IX_squads_name",
                table: "squads",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_template_columns_template_id",
                table: "template_columns",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_templates_key",
                table: "templates",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_themes_key",
                table: "themes",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "action_items");

            migrationBuilder.DropTable(
                name: "cards");

            migrationBuilder.DropTable(
                name: "comments");

            migrationBuilder.DropTable(
                name: "squads");

            migrationBuilder.DropTable(
                name: "template_columns");

            migrationBuilder.DropTable(
                name: "themes");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "columns");

            migrationBuilder.DropTable(
                name: "templates");

            migrationBuilder.DropTable(
                name: "sessions");
        }
    }
}
