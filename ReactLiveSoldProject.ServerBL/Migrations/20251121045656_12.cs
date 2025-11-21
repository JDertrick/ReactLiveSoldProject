using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReactLiveSoldProject.ServerBL.Migrations
{
    /// <inheritdoc />
    public partial class _12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventoryAudits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "Draft"),
                    snapshot_taken_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    completed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    total_variants = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    counted_variants = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    total_variance = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    total_variance_value = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0.00m),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryAudits", x => x.id);
                    table.ForeignKey(
                        name: "FK_InventoryAudits_Organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryAudits_Users_completed_by_user_id",
                        column: x => x.completed_by_user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryAudits_Users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryAuditItems",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    inventory_audit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_variant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    theoretical_stock = table.Column<int>(type: "integer", nullable: false),
                    counted_stock = table.Column<int>(type: "integer", nullable: true),
                    variance = table.Column<int>(type: "integer", nullable: true),
                    variance_value = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    snapshot_average_cost = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    counted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    counted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    adjustment_movement_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryAuditItems", x => x.id);
                    table.ForeignKey(
                        name: "FK_InventoryAuditItems_InventoryAudits_inventory_audit_id",
                        column: x => x.inventory_audit_id,
                        principalTable: "InventoryAudits",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryAuditItems_ProductVariants_product_variant_id",
                        column: x => x.product_variant_id,
                        principalTable: "ProductVariants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryAuditItems_StockMovements_adjustment_movement_id",
                        column: x => x.adjustment_movement_id,
                        principalTable: "StockMovements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InventoryAuditItems_Users_counted_by_user_id",
                        column: x => x.counted_by_user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAuditItems_adjustment_movement_id",
                table: "InventoryAuditItems",
                column: "adjustment_movement_id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAuditItems_counted_by_user_id",
                table: "InventoryAuditItems",
                column: "counted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAuditItems_inventory_audit_id",
                table: "InventoryAuditItems",
                column: "inventory_audit_id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAuditItems_inventory_audit_id_product_variant_id",
                table: "InventoryAuditItems",
                columns: new[] { "inventory_audit_id", "product_variant_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAuditItems_product_variant_id",
                table: "InventoryAuditItems",
                column: "product_variant_id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAudits_completed_by_user_id",
                table: "InventoryAudits",
                column: "completed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAudits_created_by_user_id",
                table: "InventoryAudits",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAudits_organization_id",
                table: "InventoryAudits",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAudits_status",
                table: "InventoryAudits",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryAuditItems");

            migrationBuilder.DropTable(
                name: "InventoryAudits");
        }
    }
}
