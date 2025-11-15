using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReactLiveSoldProject.ServerBL.Migrations
{
    /// <inheritdoc />
    public partial class _4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_rejected",
                table: "Receipts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "rejected_at",
                table: "Receipts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "rejected_by_user_id",
                table: "Receipts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_rejected_by_user_id",
                table: "Receipts",
                column: "rejected_by_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Users_rejected_by_user_id",
                table: "Receipts",
                column: "rejected_by_user_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Users_rejected_by_user_id",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_rejected_by_user_id",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "is_rejected",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "rejected_at",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "rejected_by_user_id",
                table: "Receipts");
        }
    }
}
