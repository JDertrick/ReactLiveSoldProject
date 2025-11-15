using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReactLiveSoldProject.ServerBL.Migrations
{
    /// <inheritdoc />
    public partial class _3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_WalletTransactions_wallet_transaction_id",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_wallet_transaction_id",
                table: "Receipts");

            migrationBuilder.AlterColumn<Guid>(
                name: "wallet_transaction_id",
                table: "Receipts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<bool>(
                name: "is_posted",
                table: "Receipts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "posted_at",
                table: "Receipts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "posted_by_user_id",
                table: "Receipts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_posted_by_user_id",
                table: "Receipts",
                column: "posted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_wallet_transaction_id",
                table: "Receipts",
                column: "wallet_transaction_id",
                unique: true,
                filter: "\"wallet_transaction_id\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Users_posted_by_user_id",
                table: "Receipts",
                column: "posted_by_user_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_WalletTransactions_wallet_transaction_id",
                table: "Receipts",
                column: "wallet_transaction_id",
                principalTable: "WalletTransactions",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Users_posted_by_user_id",
                table: "Receipts");

            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_WalletTransactions_wallet_transaction_id",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_posted_by_user_id",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_wallet_transaction_id",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "is_posted",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "posted_at",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "posted_by_user_id",
                table: "Receipts");

            migrationBuilder.AlterColumn<Guid>(
                name: "wallet_transaction_id",
                table: "Receipts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_wallet_transaction_id",
                table: "Receipts",
                column: "wallet_transaction_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_WalletTransactions_wallet_transaction_id",
                table: "Receipts",
                column: "wallet_transaction_id",
                principalTable: "WalletTransactions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
