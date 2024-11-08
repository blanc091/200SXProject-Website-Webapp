using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _200SXContact.Migrations
{
    /// <inheritdoc />
    public partial class checkoutOrderModels_addOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.CreateTable(
	 name: "OrderItems",
	 columns: table => new
	 {
		 Id = table.Column<int>(nullable: false)
			 .Annotation("SqlServer:Identity", "1, 1"),
		 OrderId = table.Column<int>(nullable: false),
		 CartItemId = table.Column<int>(nullable: false),
		 Quantity = table.Column<int>(nullable: false),
		 Price = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
		 ProductName = table.Column<string>(nullable: false)
	 },
	 constraints: table =>
	 {
		 table.PrimaryKey("PK_OrderItems", x => x.Id);
		 table.ForeignKey(
			 name: "FK_OrderItems_CartItems_CartItemId",
			 column: x => x.CartItemId,
			 principalTable: "CartItems",
			 principalColumn: "Id",
			 onDelete: ReferentialAction.Cascade);
		 table.ForeignKey(
			 name: "FK_OrderItems_Orders_OrderId",
			 column: x => x.OrderId,
			 principalTable: "Orders",
			 principalColumn: "Id",
			 onDelete: ReferentialAction.Restrict); // or SetNull
	 });

			migrationBuilder.CreateIndex(
                name: "IX_OrderItems_CartItemId",
                table: "OrderItems",
                column: "CartItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");
        }
    }
}
