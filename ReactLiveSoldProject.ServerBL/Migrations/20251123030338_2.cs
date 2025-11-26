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
            migrationBuilder.CreateTable(
                name: "ChartOfAccounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    account_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    account_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    account_type = table.Column<string>(type: "text", nullable: false),
                    system_account_type = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartOfAccounts", x => x.id);
                    table.ForeignKey(
                        name: "FK_ChartOfAccounts_Organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    reference_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntries", x => x.id);
                    table.ForeignKey(
                        name: "FK_JournalEntries_Organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntryLines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    journal_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    debit = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0.00m),
                    credit = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0.00m),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntryLines", x => x.id);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_ChartOfAccounts_account_id",
                        column: x => x.account_id,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_JournalEntries_journal_entry_id",
                        column: x => x.journal_entry_id,
                        principalTable: "JournalEntries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChartOfAccounts_organization_id_account_code",
                table: "ChartOfAccounts",
                columns: new[] { "organization_id", "account_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChartOfAccounts_organization_id_account_name",
                table: "ChartOfAccounts",
                columns: new[] { "organization_id", "account_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChartOfAccounts_organization_id_system_account_type",
                table: "ChartOfAccounts",
                columns: new[] { "organization_id", "system_account_type" },
                unique: true,
                filter: "\"system_account_type\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_entry_date",
                table: "JournalEntries",
                column: "entry_date");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_organization_id",
                table: "JournalEntries",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_account_id",
                table: "JournalEntryLines",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_journal_entry_id",
                table: "JournalEntryLines",
                column: "journal_entry_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JournalEntryLines");

            migrationBuilder.DropTable(
                name: "ChartOfAccounts");

            migrationBuilder.DropTable(
                name: "JournalEntries");
        }
    }
}
