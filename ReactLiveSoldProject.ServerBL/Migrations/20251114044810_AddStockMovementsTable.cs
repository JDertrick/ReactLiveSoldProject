using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReactLiveSoldProject.ServerBL.Migrations
{
    /// <inheritdoc />
    public partial class AddStockMovementsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockMovements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_variant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    movement_type = table.Column<string>(type: "text", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    stock_before = table.Column<int>(type: "integer", nullable: false),
                    stock_after = table.Column<int>(type: "integer", nullable: false),
                    related_sales_order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    unit_cost = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.id);
                    table.ForeignKey(
                        name: "FK_StockMovements_Organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockMovements_ProductVariants_product_variant_id",
                        column: x => x.product_variant_id,
                        principalTable: "ProductVariants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockMovements_SalesOrders_related_sales_order_id",
                        column: x => x.related_sales_order_id,
                        principalTable: "SalesOrders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockMovements_Users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_created_at",
                table: "StockMovements",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_created_by_user_id",
                table: "StockMovements",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_organization_id",
                table: "StockMovements",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_product_variant_id",
                table: "StockMovements",
                column: "product_variant_id");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_related_sales_order_id",
                table: "StockMovements",
                column: "related_sales_order_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockMovements");
        }
    }
}
