using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIssueGeoLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Issues",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Issues",
                type: "REAL",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Issues");
        }
    }
}
