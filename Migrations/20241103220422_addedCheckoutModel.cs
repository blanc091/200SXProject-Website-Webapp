using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _200SXContact.Migrations
{
    /// <inheritdoc />
    public partial class addedCheckoutModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CheckoutId",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Checkouts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    County = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checkouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Checkouts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CheckoutId",
                table: "CartItems",
                column: "CheckoutId");

            migrationBuilder.CreateIndex(
                name: "IX_Checkouts_UserId",
                table: "Checkouts",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Checkouts_CheckoutId",
                table: "CartItems",
                column: "CheckoutId",
                principalTable: "Checkouts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Checkouts_CheckoutId",
                table: "CartItems");

            migrationBuilder.DropTable(
                name: "Checkouts");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CheckoutId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "CheckoutId",
                table: "CartItems");
        }
    }
}
