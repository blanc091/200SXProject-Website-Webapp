using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _200SXContact.Migrations
{
    /// <inheritdoc />
    public partial class FixForOrderFlow2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_OrderTrackings_OrderTrackingId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Orders_OrderId",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_OrderId",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_OrderTrackingId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "OrderTrackingId",
                table: "CartItems");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_OrderPlacementId",
                table: "CartItems",
                column: "OrderPlacementId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Orders_OrderPlacementId",
                table: "CartItems",
                column: "OrderPlacementId",
                principalTable: "Orders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Orders_OrderPlacementId",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_OrderPlacementId",
                table: "CartItems");

            migrationBuilder.AddColumn<int>(
                name: "OrderTrackingId",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_OrderId",
                table: "CartItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_OrderTrackingId",
                table: "CartItems",
                column: "OrderTrackingId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_OrderTrackings_OrderTrackingId",
                table: "CartItems",
                column: "OrderTrackingId",
                principalTable: "OrderTrackings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Orders_OrderId",
                table: "CartItems",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
