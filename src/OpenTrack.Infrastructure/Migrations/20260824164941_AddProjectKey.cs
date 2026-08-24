using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "Projects",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Key",
                table: "Projects");
        }
    }
}
