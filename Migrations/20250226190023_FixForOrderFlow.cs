using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _200SXContact.Migrations
{
    /// <inheritdoc />
    public partial class FixForOrderFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderTrackings_OrderId",
                table: "OrderTrackings");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTrackings_OrderId",
                table: "OrderTrackings",
                column: "OrderId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderTrackings_OrderId",
                table: "OrderTrackings");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTrackings_OrderId",
                table: "OrderTrackings",
                column: "OrderId");
        }
    }
}
