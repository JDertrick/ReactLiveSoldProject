using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReactLiveSoldProject.ServerBL.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationAccountConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrganizationAccountConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    AccountsPayableAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    AccountsReceivableAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    SalesRevenueAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    CostOfGoodsSoldAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    TaxPayableAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    TaxReceivableAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    CashAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    DefaultBankAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationAccountConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationAccountConfigurations_ChartOfAccounts_AccountsP~",
                        column: x => x.AccountsPayableAccountId,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_OrganizationAccountConfigurations_ChartOfAccounts_AccountsR~",
                        column: x => x.AccountsReceivableAccountId,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_OrganizationAccountConfigurations_ChartOfAccounts_CashAccou~",
                        column: x => x.CashAccountId,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_OrganizationAccountConfigurations_ChartOfAccounts_CostOfGoo~",
                        column: x => x.CostOfGoodsSoldAccountId,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_OrganizationAccountConfigurations_ChartOfAccounts_DefaultBa~",
                        column: x => x.DefaultBankAccountId,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_OrganizationAccountConfigurations_ChartOfAccounts_Inventory~",
                        column: x => x.InventoryAccountId,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_OrganizationAccountConfigurations_ChartOfAccounts_SalesReve~",
                        column: x => x.SalesRevenueAccountId,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_OrganizationAccountConfigurations_ChartOfAccounts_TaxPayabl~",
                        column: x => x.TaxPayableAccountId,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_OrganizationAccountConfigurations_ChartOfAccounts_TaxReceiv~",
                        column: x => x.TaxReceivableAccountId,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationAccountConfigurations_AccountsPayableAccountId",
                table: "OrganizationAccountConfigurations",
                column: "AccountsPayableAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationAccountConfigurations_AccountsReceivableAccount~",
                table: "OrganizationAccountConfigurations",
                column: "AccountsReceivableAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationAccountConfigurations_CashAccountId",
                table: "OrganizationAccountConfigurations",
                column: "CashAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationAccountConfigurations_CostOfGoodsSoldAccountId",
                table: "OrganizationAccountConfigurations",
                column: "CostOfGoodsSoldAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationAccountConfigurations_DefaultBankAccountId",
                table: "OrganizationAccountConfigurations",
                column: "DefaultBankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationAccountConfigurations_InventoryAccountId",
                table: "OrganizationAccountConfigurations",
                column: "InventoryAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationAccountConfigurations_SalesRevenueAccountId",
                table: "OrganizationAccountConfigurations",
                column: "SalesRevenueAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationAccountConfigurations_TaxPayableAccountId",
                table: "OrganizationAccountConfigurations",
                column: "TaxPayableAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationAccountConfigurations_TaxReceivableAccountId",
                table: "OrganizationAccountConfigurations",
                column: "TaxReceivableAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizationAccountConfigurations");
        }
    }
}
