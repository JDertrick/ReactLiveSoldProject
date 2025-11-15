using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReactLiveSoldProject.ServerBL.Migrations
{
    /// <inheritdoc />
    public partial class _5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Users_posted_by_user_id",
                table: "StockMovements");

            migrationBuilder.AddColumn<bool>(
                name: "is_rejected",
                table: "StockMovements",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "rejected_at",
                table: "StockMovements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "rejected_by_user_id",
                table: "StockMovements",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_rejected_by_user_id",
                table: "StockMovements",
                column: "rejected_by_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Users_posted_by_user_id",
                table: "StockMovements",
                column: "posted_by_user_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Users_rejected_by_user_id",
                table: "StockMovements",
                column: "rejected_by_user_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Users_posted_by_user_id",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Users_rejected_by_user_id",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_rejected_by_user_id",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "is_rejected",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "rejected_at",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "rejected_by_user_id",
                table: "StockMovements");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Users_posted_by_user_id",
                table: "StockMovements",
                column: "posted_by_user_id",
                principalTable: "Users",
                principalColumn: "id");
        }
    }
}
