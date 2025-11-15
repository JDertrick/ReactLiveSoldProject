using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReactLiveSoldProject.ServerBL.Migrations
{
    /// <inheritdoc />
    public partial class AddCostTrackingAndPostingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPosted",
                table: "StockMovements",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PostedAt",
                table: "StockMovements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PostedByUserId",
                table: "StockMovements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                table: "SalesOrderItems",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageCost",
                table: "ProductVariants",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_PostedByUserId",
                table: "StockMovements",
                column: "PostedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Users_PostedByUserId",
                table: "StockMovements",
                column: "PostedByUserId",
                principalTable: "Users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Users_PostedByUserId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_PostedByUserId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "IsPosted",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "PostedAt",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "PostedByUserId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "UnitCost",
                table: "SalesOrderItems");

            migrationBuilder.DropColumn(
                name: "AverageCost",
                table: "ProductVariants");
        }
    }
}
