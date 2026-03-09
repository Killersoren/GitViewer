using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GitViewer.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPublicBoolToRepo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Repository",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Repository");
        }
    }
}
