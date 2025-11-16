using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReactLiveSoldProject.ServerBL.Migrations
{
    /// <inheritdoc />
    public partial class _8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Locations_location_id",
                table: "StockMovements");

            migrationBuilder.RenameColumn(
                name: "location_id",
                table: "StockMovements",
                newName: "source_location_id");

            migrationBuilder.RenameIndex(
                name: "IX_StockMovements_location_id",
                table: "StockMovements",
                newName: "IX_StockMovements_source_location_id");

            migrationBuilder.AddColumn<Guid>(
                name: "destination_location_id",
                table: "StockMovements",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_destination_location_id",
                table: "StockMovements",
                column: "destination_location_id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Locations_destination_location_id",
                table: "StockMovements",
                column: "destination_location_id",
                principalTable: "Locations",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Locations_source_location_id",
                table: "StockMovements",
                column: "source_location_id",
                principalTable: "Locations",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Locations_destination_location_id",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Locations_source_location_id",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_destination_location_id",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "destination_location_id",
                table: "StockMovements");

            migrationBuilder.RenameColumn(
                name: "source_location_id",
                table: "StockMovements",
                newName: "location_id");

            migrationBuilder.RenameIndex(
                name: "IX_StockMovements_source_location_id",
                table: "StockMovements",
                newName: "IX_StockMovements_location_id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Locations_location_id",
                table: "StockMovements",
                column: "location_id",
                principalTable: "Locations",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
