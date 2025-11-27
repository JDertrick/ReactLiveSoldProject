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
                name: "FK_PurchaseOrders_Users_created_by",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReceipts_Users_ReceivedByUserId",
                table: "PurchaseReceipts");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReceipts_ReceivedByUserId",
                table: "PurchaseReceipts");

            migrationBuilder.DropColumn(
                name: "ReceivedByUserId",
                table: "PurchaseReceipts");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceipts_ReceivedBy",
                table: "PurchaseReceipts",
                column: "ReceivedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Users_CreatedByUserId",
                table: "PurchaseOrders",
                column: "created_by",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReceipts_Users_ReceivedByUserId",
                table: "PurchaseReceipts",
                column: "ReceivedBy",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Users_CreatedByUserId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReceipts_Users_ReceivedByUserId",
                table: "PurchaseReceipts");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReceipts_ReceivedBy",
                table: "PurchaseReceipts");

            migrationBuilder.AddColumn<Guid>(
                name: "ReceivedByUserId",
                table: "PurchaseReceipts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceipts_ReceivedByUserId",
                table: "PurchaseReceipts",
                column: "ReceivedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Users_created_by",
                table: "PurchaseOrders",
                column: "created_by",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReceipts_Users_ReceivedByUserId",
                table: "PurchaseReceipts",
                column: "ReceivedByUserId",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
