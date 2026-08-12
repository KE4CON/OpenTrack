using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSlaPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SlaBreachNotifiedAt",
                table: "Issues",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SlaPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetHours = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlaPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlaPolicies_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SlaPolicies_ProjectId_Priority",
                table: "SlaPolicies",
                columns: new[] { "ProjectId", "Priority" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SlaPolicies");

            migrationBuilder.DropColumn(
                name: "SlaBreachNotifiedAt",
                table: "Issues");
        }
    }
}
