using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GitViewer.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedSourceToRepository : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Repository",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Source",
                table: "Repository");
        }
    }
}
