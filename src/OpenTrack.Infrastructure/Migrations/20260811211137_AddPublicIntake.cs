using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicIntake : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PublicIntakeEnabled",
                table: "Projects",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IntakeEmail",
                table: "Issues",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IntakeName",
                table: "Issues",
                type: "TEXT",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicIntakeEnabled",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "IntakeEmail",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "IntakeName",
                table: "Issues");
        }
    }
}
