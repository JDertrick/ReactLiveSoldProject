using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReactLiveSoldProject.ServerBL.Migrations
{
    /// <inheritdoc />
    public partial class _7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Locations_location_id",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "location_id",
                table: "Products",
                newName: "LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_location_id",
                table: "Products",
                newName: "IX_Products_LocationId");

            migrationBuilder.AddColumn<Guid>(
                name: "location_id",
                table: "StockMovements",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_location_id",
                table: "StockMovements",
                column: "location_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Locations_LocationId",
                table: "Products",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Locations_location_id",
                table: "StockMovements",
                column: "location_id",
                principalTable: "Locations",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Locations_LocationId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Locations_location_id",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_location_id",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "location_id",
                table: "StockMovements");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "Products",
                newName: "location_id");

            migrationBuilder.RenameIndex(
                name: "IX_Products_LocationId",
                table: "Products",
                newName: "IX_Products_location_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Locations_location_id",
                table: "Products",
                column: "location_id",
                principalTable: "Locations",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
