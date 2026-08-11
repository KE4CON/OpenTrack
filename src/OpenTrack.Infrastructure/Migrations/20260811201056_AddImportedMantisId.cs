using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImportedMantisId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImportedMantisId",
                table: "Issues",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ProjectId_ImportedMantisId",
                table: "Issues",
                columns: new[] { "ProjectId", "ImportedMantisId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Issues_ProjectId_ImportedMantisId",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "ImportedMantisId",
                table: "Issues");
        }
    }
}
