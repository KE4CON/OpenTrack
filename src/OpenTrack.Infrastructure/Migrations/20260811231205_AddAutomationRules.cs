using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAutomationRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutomationRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    WhenTextContains = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    WhenSeverity = table.Column<int>(type: "INTEGER", nullable: true),
                    WhenPriority = table.Column<int>(type: "INTEGER", nullable: true),
                    WhenCategoryId = table.Column<int>(type: "INTEGER", nullable: true),
                    SetSeverity = table.Column<int>(type: "INTEGER", nullable: true),
                    SetPriority = table.Column<int>(type: "INTEGER", nullable: true),
                    SetStatus = table.Column<int>(type: "INTEGER", nullable: true),
                    AssignToUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    AddTag = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomationRules_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRules_ProjectId_SortOrder",
                table: "AutomationRules",
                columns: new[] { "ProjectId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutomationRules");
        }
    }
}
