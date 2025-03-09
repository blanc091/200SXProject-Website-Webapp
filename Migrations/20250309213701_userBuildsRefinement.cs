using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _200SXContact.Migrations
{
    /// <inheritdoc />
    public partial class userBuildsRefinement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserBuilds_AspNetUsers_UserId",
                table: "UserBuilds");

            migrationBuilder.AddForeignKey(
                name: "FK_UserBuilds_AspNetUsers_UserId",
                table: "UserBuilds",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserBuilds_AspNetUsers_UserId",
                table: "UserBuilds");

            migrationBuilder.AddForeignKey(
                name: "FK_UserBuilds_AspNetUsers_UserId",
                table: "UserBuilds",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
