using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReactLiveSoldProject.ServerBL.Migrations
{
    /// <inheritdoc />
    public partial class _11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "subtotal_amount",
                table: "SalesOrders",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0.00m);

            migrationBuilder.AddColumn<decimal>(
                name: "total_tax_amount",
                table: "SalesOrders",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0.00m);

            migrationBuilder.AddColumn<decimal>(
                name: "subtotal",
                table: "SalesOrderItems",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0.00m);

            migrationBuilder.AddColumn<decimal>(
                name: "tax_amount",
                table: "SalesOrderItems",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0.00m);

            migrationBuilder.AddColumn<decimal>(
                name: "tax_rate",
                table: "SalesOrderItems",
                type: "numeric(5,4)",
                nullable: false,
                defaultValue: 0.00m);

            migrationBuilder.AddColumn<Guid>(
                name: "tax_rate_id",
                table: "SalesOrderItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total",
                table: "SalesOrderItems",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0.00m);

            migrationBuilder.AddColumn<bool>(
                name: "is_tax_exempt",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "default_tax_rate_id",
                table: "Organizations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tax_application_mode",
                table: "Organizations",
                type: "text",
                nullable: false,
                defaultValue: "TaxIncluded");

            migrationBuilder.AddColumn<string>(
                name: "tax_display_name",
                table: "Organizations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "tax_enabled",
                table: "Organizations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "tax_system_type",
                table: "Organizations",
                type: "text",
                nullable: false,
                defaultValue: "None");

            migrationBuilder.CreateTable(
                name: "tax_rates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    rate = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    effective_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    effective_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tax_rates", x => x.id);
                    table.ForeignKey(
                        name: "FK_tax_rates_Organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderItems_tax_rate_id",
                table: "SalesOrderItems",
                column: "tax_rate_id");

            migrationBuilder.CreateIndex(
                name: "IX_tax_rates_organization_id",
                table: "tax_rates",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_tax_rates_organization_id_is_default",
                table: "tax_rates",
                columns: new[] { "organization_id", "is_default" });

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderItems_tax_rates_tax_rate_id",
                table: "SalesOrderItems",
                column: "tax_rate_id",
                principalTable: "tax_rates",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderItems_tax_rates_tax_rate_id",
                table: "SalesOrderItems");

            migrationBuilder.DropTable(
                name: "tax_rates");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderItems_tax_rate_id",
                table: "SalesOrderItems");

            migrationBuilder.DropColumn(
                name: "subtotal_amount",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "total_tax_amount",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "subtotal",
                table: "SalesOrderItems");

            migrationBuilder.DropColumn(
                name: "tax_amount",
                table: "SalesOrderItems");

            migrationBuilder.DropColumn(
                name: "tax_rate",
                table: "SalesOrderItems");

            migrationBuilder.DropColumn(
                name: "tax_rate_id",
                table: "SalesOrderItems");

            migrationBuilder.DropColumn(
                name: "total",
                table: "SalesOrderItems");

            migrationBuilder.DropColumn(
                name: "is_tax_exempt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "default_tax_rate_id",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "tax_application_mode",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "tax_display_name",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "tax_enabled",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "tax_system_type",
                table: "Organizations");
        }
    }
}
