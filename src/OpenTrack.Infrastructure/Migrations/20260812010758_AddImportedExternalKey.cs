using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImportedExternalKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImportedExternalKey",
                table: "Issues",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ProjectId_ImportedExternalKey",
                table: "Issues",
                columns: new[] { "ProjectId", "ImportedExternalKey" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Issues_ProjectId_ImportedExternalKey",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "ImportedExternalKey",
                table: "Issues");
        }
    }
}
