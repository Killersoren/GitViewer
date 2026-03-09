using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GitViewer.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToRepository : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Repository",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Repository");
        }
    }
}
