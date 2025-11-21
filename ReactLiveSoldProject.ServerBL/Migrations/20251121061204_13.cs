using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReactLiveSoldProject.ServerBL.Migrations
{
    /// <inheritdoc />
    public partial class _13 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "InventoryAudits",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScopeDescription",
                table: "InventoryAudits",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScopeType",
                table: "InventoryAudits",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAudits_LocationId",
                table: "InventoryAudits",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryAudits_Locations_LocationId",
                table: "InventoryAudits",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryAudits_Locations_LocationId",
                table: "InventoryAudits");

            migrationBuilder.DropIndex(
                name: "IX_InventoryAudits_LocationId",
                table: "InventoryAudits");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "InventoryAudits");

            migrationBuilder.DropColumn(
                name: "ScopeDescription",
                table: "InventoryAudits");

            migrationBuilder.DropColumn(
                name: "ScopeType",
                table: "InventoryAudits");
        }
    }
}
