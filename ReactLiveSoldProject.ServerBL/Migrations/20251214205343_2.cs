using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReactLiveSoldProject.ServerBL.Migrations
{
    /// <inheritdoc />
    public partial class _2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "plan_type",
                table: "Organizations",
                type: "text",
                nullable: false,
                defaultValue: "Free",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Standard");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "no_series",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "document_type",
                table: "no_series",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "no_series",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                table: "no_series");

            migrationBuilder.DropColumn(
                name: "document_type",
                table: "no_series");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "no_series");

            migrationBuilder.AlterColumn<string>(
                name: "plan_type",
                table: "Organizations",
                type: "text",
                nullable: false,
                defaultValue: "Standard",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Free");
        }
    }
}
