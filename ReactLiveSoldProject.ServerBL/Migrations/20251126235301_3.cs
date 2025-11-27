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
                name: "FK_PurchaseOrders_Organizations_OrganizationId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_PaymentTerms_PaymentTermsId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Users_CreatedByUserId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Vendors_VendorId",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_CreatedByUserId",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "PurchaseOrders");

            migrationBuilder.RenameColumn(
                name: "Subtotal",
                table: "PurchaseOrders",
                newName: "subtotal");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "PurchaseOrders",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "PurchaseOrders",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "Currency",
                table: "PurchaseOrders",
                newName: "currency");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "PurchaseOrders",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "VendorId",
                table: "PurchaseOrders",
                newName: "vendor_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "PurchaseOrders",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "PurchaseOrders",
                newName: "total_amount");

            migrationBuilder.RenameColumn(
                name: "TaxAmount",
                table: "PurchaseOrders",
                newName: "tax_amount");

            migrationBuilder.RenameColumn(
                name: "PaymentTermsId",
                table: "PurchaseOrders",
                newName: "payment_terms_id");

            migrationBuilder.RenameColumn(
                name: "PONumber",
                table: "PurchaseOrders",
                newName: "po_number");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "PurchaseOrders",
                newName: "organization_id");

            migrationBuilder.RenameColumn(
                name: "OrderDate",
                table: "PurchaseOrders",
                newName: "order_date");

            migrationBuilder.RenameColumn(
                name: "ExpectedDeliveryDate",
                table: "PurchaseOrders",
                newName: "expected_delivery_date");

            migrationBuilder.RenameColumn(
                name: "ExchangeRate",
                table: "PurchaseOrders",
                newName: "exchange_rate");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "PurchaseOrders",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "PurchaseOrders",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrders_VendorId",
                table: "PurchaseOrders",
                newName: "IX_PurchaseOrders_vendor_id");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrders_PaymentTermsId",
                table: "PurchaseOrders",
                newName: "IX_PurchaseOrders_payment_terms_id");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrders_OrganizationId",
                table: "PurchaseOrders",
                newName: "IX_PurchaseOrders_organization_id");

            migrationBuilder.AlterColumn<decimal>(
                name: "subtotal",
                table: "PurchaseOrders",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "PurchaseOrders",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "currency",
                table: "PurchaseOrders",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "MXN",
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "PurchaseOrders",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "PurchaseOrders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "(now() at time zone 'utc')",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<decimal>(
                name: "total_amount",
                table: "PurchaseOrders",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "tax_amount",
                table: "PurchaseOrders",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "exchange_rate",
                table: "PurchaseOrders",
                type: "numeric(18,6)",
                nullable: false,
                defaultValue: 1.0m,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "PurchaseOrders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "(now() at time zone 'utc')",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateTable(
                name: "PurchaseOrderItems",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    purchase_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_number = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_variant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_cost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    discount_percentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    discount_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    tax_rate = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    tax_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    line_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderItems", x => x.id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderItems_ProductVariants_product_variant_id",
                        column: x => x.product_variant_id,
                        principalTable: "ProductVariants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderItems_Products_product_id",
                        column: x => x.product_id,
                        principalTable: "Products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderItems_PurchaseOrders_purchase_order_id",
                        column: x => x.purchase_order_id,
                        principalTable: "PurchaseOrders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_created_by",
                table: "PurchaseOrders",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_organization_id_po_number",
                table: "PurchaseOrders",
                columns: new[] { "organization_id", "po_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_product_id",
                table: "PurchaseOrderItems",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_product_variant_id",
                table: "PurchaseOrderItems",
                column: "product_variant_id");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_purchase_order_id",
                table: "PurchaseOrderItems",
                column: "purchase_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_purchase_order_id_line_number",
                table: "PurchaseOrderItems",
                columns: new[] { "purchase_order_id", "line_number" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Organizations_organization_id",
                table: "PurchaseOrders",
                column: "organization_id",
                principalTable: "Organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_PaymentTerms_payment_terms_id",
                table: "PurchaseOrders",
                column: "payment_terms_id",
                principalTable: "PaymentTerms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Users_created_by",
                table: "PurchaseOrders",
                column: "created_by",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Vendors_vendor_id",
                table: "PurchaseOrders",
                column: "vendor_id",
                principalTable: "Vendors",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Organizations_organization_id",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_PaymentTerms_payment_terms_id",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Users_created_by",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Vendors_vendor_id",
                table: "PurchaseOrders");

            migrationBuilder.DropTable(
                name: "PurchaseOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_created_by",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_organization_id_po_number",
                table: "PurchaseOrders");

            migrationBuilder.RenameColumn(
                name: "subtotal",
                table: "PurchaseOrders",
                newName: "Subtotal");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "PurchaseOrders",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "PurchaseOrders",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "currency",
                table: "PurchaseOrders",
                newName: "Currency");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "PurchaseOrders",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "vendor_id",
                table: "PurchaseOrders",
                newName: "VendorId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "PurchaseOrders",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "total_amount",
                table: "PurchaseOrders",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "tax_amount",
                table: "PurchaseOrders",
                newName: "TaxAmount");

            migrationBuilder.RenameColumn(
                name: "po_number",
                table: "PurchaseOrders",
                newName: "PONumber");

            migrationBuilder.RenameColumn(
                name: "payment_terms_id",
                table: "PurchaseOrders",
                newName: "PaymentTermsId");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                table: "PurchaseOrders",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "order_date",
                table: "PurchaseOrders",
                newName: "OrderDate");

            migrationBuilder.RenameColumn(
                name: "expected_delivery_date",
                table: "PurchaseOrders",
                newName: "ExpectedDeliveryDate");

            migrationBuilder.RenameColumn(
                name: "exchange_rate",
                table: "PurchaseOrders",
                newName: "ExchangeRate");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "PurchaseOrders",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "PurchaseOrders",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrders_vendor_id",
                table: "PurchaseOrders",
                newName: "IX_PurchaseOrders_VendorId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrders_payment_terms_id",
                table: "PurchaseOrders",
                newName: "IX_PurchaseOrders_PaymentTermsId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrders_organization_id",
                table: "PurchaseOrders",
                newName: "IX_PurchaseOrders_OrganizationId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Subtotal",
                table: "PurchaseOrders",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "PurchaseOrders",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "PurchaseOrders",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldDefaultValue: "MXN");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "PurchaseOrders",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "PurchaseOrders",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "(now() at time zone 'utc')");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                table: "PurchaseOrders",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxAmount",
                table: "PurchaseOrders",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "PurchaseOrders",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)",
                oldDefaultValue: 1.0m);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PurchaseOrders",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "(now() at time zone 'utc')");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "PurchaseOrders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_CreatedByUserId",
                table: "PurchaseOrders",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Organizations_OrganizationId",
                table: "PurchaseOrders",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_PaymentTerms_PaymentTermsId",
                table: "PurchaseOrders",
                column: "PaymentTermsId",
                principalTable: "PaymentTerms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Users_CreatedByUserId",
                table: "PurchaseOrders",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Vendors_VendorId",
                table: "PurchaseOrders",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
