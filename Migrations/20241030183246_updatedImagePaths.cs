using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _200SXContact.Migrations
{
    /// <inheritdoc />
    public partial class updatedImagePaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "UserBuilds",
                newName: "ImagePaths");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImagePaths",
                table: "UserBuilds",
                newName: "ImagePath");
        }
    }
}
