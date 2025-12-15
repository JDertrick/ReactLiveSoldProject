using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReactLiveSoldProject.ServerBL.Migrations
{
    /// <inheritdoc />
    public partial class _1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "no_series",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    document_type = table.Column<int>(type: "integer", nullable: false),
                    default_nos = table.Column<bool>(type: "boolean", nullable: false),
                    manual_nos = table.Column<bool>(type: "boolean", nullable: false),
                    date_order = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_no_series", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    logo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    primary_contact_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    plan_type = table.Column<string>(type: "text", nullable: false, defaultValue: "Free"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    custom_settings = table.Column<string>(type: "text", nullable: false, defaultValue: "True"),
                    tax_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    tax_system_type = table.Column<string>(type: "text", nullable: false, defaultValue: "None"),
                    tax_display_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    tax_application_mode = table.Column<string>(type: "text", nullable: false, defaultValue: "TaxIncluded"),
                    default_tax_rate_id = table.Column<Guid>(type: "uuid", nullable: true),
                    CostMethod = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    is_super_admin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "no_serie_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    NoSerieId = table.Column<Guid>(type: "uuid", nullable: false),
                    starting_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    starting_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ending_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    last_no_used = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    last_date_used = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    manual_nos = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    warning_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    open = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_no_serie_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_no_serie_lines_no_series_NoSerieId",
                        column: x => x.NoSerieId,
                        principalTable: "no_series",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_parent_id",
                        column: x => x.parent_id,
                        principalTable: "Categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Categories_Organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ParentAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartOfAccounts", x => x.id);
                    table.ForeignKey(
                        name: "FK_ChartOfAccounts_ChartOfAccounts_ParentAccountId",
                        column: x => x.ParentAccountId,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ChartOfAccounts_Organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    company = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    job_title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.id);
                    table.ForeignKey(
                        name: "FK_Contacts_Organizations_organization_id",
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
                    EntryNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    reference_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DocumentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    VendorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PostedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    PostedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
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
                name: "Locations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.id);
                    table.ForeignKey(
                        name: "FK_Locations_Organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTerms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DueDays = table.Column<int>(type: "integer", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    DiscountDays = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTerms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentTerms_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.id);
                    table.ForeignKey(
                        name: "FK_Tags_Organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateTable(
                name: "ApprovalWorkflows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApproverId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResponseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Comments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalWorkflows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalWorkflows_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApprovalWorkflows_Users_ApproverId",
                        column: x => x.ApproverId,
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ApprovalWorkflows_Users_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action_type = table.Column<string>(type: "text", nullable: false),
                    target_table = table.Column<string>(type: "text", nullable: false),
                    target_record_id = table.Column<Guid>(type: "uuid", nullable: true),
                    changes = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.id);
                    table.ForeignKey(
                        name: "FK_AuditLog_Organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AuditLog_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    message = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationMembers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false, defaultValue: "Seller"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationMembers", x => x.id);
                    table.ForeignKey(
                        name: "FK_OrganizationMembers_Organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationMembers_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "company_bank_accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    account_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    current_balance = table.Column<decimal>(type: "numeric", nullable: false),
                    gl_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_company_bank_accounts", x => x.id);
                    table.ForeignKey(
                        name: "FK_company_bank_accounts_ChartOfAccounts_gl_account_id",
                        column: x => x.gl_account_id,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_company_bank_accounts_Organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    contact_id = table.Column<Guid>(type: "uuid", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    assigned_seller_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.id);
                    table.ForeignKey(
                        name: "FK_Customers_Contacts_contact_id",
                        column: x => x.contact_id,
                        principalTable: "Contacts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Customers_Organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Customers_Users_assigned_seller_id",
                        column: x => x.assigned_seller_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Vendors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    contact_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_buyer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    vendor_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    payment_terms = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TaxId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PaymentTermsId = table.Column<Guid>(type: "uuid", nullable: true),
                    credit_limit = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0.00m),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendors", x => x.id);
                    table.ForeignKey(
                        name: "FK_Vendors_Contacts_contact_id",
                        column: x => x.contact_id,
                        principalTable: "Contacts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vendors_Organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vendors_Users_assigned_buyer_id",
                        column: x => x.assigned_buyer_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntryLines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    journal_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    debit = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0.00m),
                    credit = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0.00m),
                    VendorId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    ScopeType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScopeDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryAudits", x => x.id);
                    table.ForeignKey(
                        name: "FK_InventoryAudits_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "id");
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
                name: "Products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    product_type = table.Column<string>(type: "text", nullable: false, defaultValue: "Simple"),
                    base_price = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 0m),
                    wholesale_price = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_tax_exempt = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ReorderPoint = table.Column<int>(type: "integer", nullable: false),
                    CostMethod = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_category_id",
                        column: x => x.category_id,
                        principalTable: "Categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Products_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Products_Organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "Draft"),
                    total_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0.00m),
                    subtotal_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0.00m),
                    total_tax_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0.00m),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrders", x => x.id);
                    table.ForeignKey(
                        name: "FK_SalesOrders_Customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "Customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesOrders_Organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesOrders_Users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    balance = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0.00m),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.id);
                    table.ForeignKey(
                        name: "FK_Wallets_Customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "Customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Wallets_Organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    po_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    vendor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expected_delivery_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    tax_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "MXN"),
                    exchange_rate = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 1.0m),
                    payment_terms_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrders", x => x.id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_PaymentTerms_payment_terms_id",
                        column: x => x.payment_terms_id,
                        principalTable: "PaymentTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Users_CreatedByUserId",
                        column: x => x.created_by,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Vendors_vendor_id",
                        column: x => x.vendor_id,
                        principalTable: "Vendors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vendor_bank_accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vendor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    account_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    account_holder_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    clabe_iban = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    account_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vendor_bank_accounts", x => x.id);
                    table.ForeignKey(
                        name: "FK_vendor_bank_accounts_Vendors_vendor_id",
                        column: x => x.vendor_id,
                        principalTable: "Vendors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductTags",
                columns: table => new
                {
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTags", x => new { x.product_id, x.tag_id });
                    table.ForeignKey(
                        name: "FK_ProductTags_Products_product_id",
                        column: x => x.product_id,
                        principalTable: "Products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductTags_Tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "Tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductVariants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    VariantNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    price = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0.00m),
                    stock_quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    attributes = table.Column<string>(type: "jsonb", nullable: true),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    size = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    color = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    wholesale_price = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    average_cost = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariants", x => x.id);
                    table.ForeignKey(
                        name: "FK_ProductVariants_Organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductVariants_Products_product_id",
                        column: x => x.product_id,
                        principalTable: "Products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductVendors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorSKU = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CostPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    LeadTimeDays = table.Column<int>(type: "integer", nullable: false),
                    MinimumOrderQuantity = table.Column<int>(type: "integer", nullable: false),
                    IsPreferred = table.Column<bool>(type: "boolean", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVendors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVendors_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductVendors_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WalletTransactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    related_sales_order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceiptId = table.Column<Guid>(type: "uuid", nullable: true),
                    authorized_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_posted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    BalanceBefore = table.Column<decimal>(type: "numeric", nullable: true),
                    BalanceAfter = table.Column<decimal>(type: "numeric", nullable: true),
                    posted_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    posted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTransactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_Organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_SalesOrders_related_sales_order_id",
                        column: x => x.related_sales_order_id,
                        principalTable: "SalesOrders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_Users_authorized_by_user_id",
                        column: x => x.authorized_by_user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_Users_posted_by_user_id",
                        column: x => x.posted_by_user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_Wallets_wallet_id",
                        column: x => x.wallet_id,
                        principalTable: "Wallets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReceipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiptNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    VendorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiptDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    WarehouseLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReceivingJournalEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseReceipts_JournalEntries_ReceivingJournalEntryId",
                        column: x => x.ReceivingJournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_PurchaseReceipts_Locations_WarehouseLocationId",
                        column: x => x.WarehouseLocationId,
                        principalTable: "Locations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_PurchaseReceipts_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseReceipts_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_PurchaseReceipts_Users_ReceivedByUserId",
                        column: x => x.ReceivedBy,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseReceipts_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    payment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    vendor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_method = table.Column<string>(type: "text", nullable: false),
                    company_bank_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vendor_bank_account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    amount_paid = table.Column<decimal>(type: "numeric", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    exchange_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    reference_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    payment_journal_entry_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.id);
                    table.ForeignKey(
                        name: "FK_Payments_Users_CreatedByUserId",
                        column: x => x.created_by_user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payments_JournalEntries_payment_journal_entry_id",
                        column: x => x.payment_journal_entry_id,
                        principalTable: "JournalEntries",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_payments_Organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_payments_Vendors_vendor_id",
                        column: x => x.vendor_id,
                        principalTable: "Vendors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payments_company_bank_accounts_company_bank_account_id",
                        column: x => x.company_bank_account_id,
                        principalTable: "company_bank_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payments_vendor_bank_accounts_vendor_bank_account_id",
                        column: x => x.vendor_bank_account_id,
                        principalTable: "vendor_bank_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateTable(
                name: "SalesOrderItems",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sales_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_variant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    original_price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    unit_cost = table.Column<decimal>(type: "numeric", nullable: false),
                    item_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    tax_rate_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tax_rate = table.Column<decimal>(type: "numeric(5,4)", nullable: false, defaultValue: 0.00m),
                    tax_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0.00m),
                    subtotal = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0.00m),
                    total = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0.00m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderItems", x => x.id);
                    table.ForeignKey(
                        name: "FK_SalesOrderItems_Organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesOrderItems_ProductVariants_product_variant_id",
                        column: x => x.product_variant_id,
                        principalTable: "ProductVariants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesOrderItems_SalesOrders_sales_order_id",
                        column: x => x.sales_order_id,
                        principalTable: "SalesOrders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesOrderItems_tax_rates_tax_rate_id",
                        column: x => x.tax_rate_id,
                        principalTable: "tax_rates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockMovements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    MovementNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    source_location_id = table.Column<Guid>(type: "uuid", nullable: true),
                    destination_location_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_posted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    posted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    posted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_rejected = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    rejected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rejected_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.id);
                    table.ForeignKey(
                        name: "FK_StockMovements_Locations_destination_location_id",
                        column: x => x.destination_location_id,
                        principalTable: "Locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StockMovements_Locations_source_location_id",
                        column: x => x.source_location_id,
                        principalTable: "Locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
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
                    table.ForeignKey(
                        name: "FK_StockMovements_Users_posted_by_user_id",
                        column: x => x.posted_by_user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockMovements_Users_rejected_by_user_id",
                        column: x => x.rejected_by_user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Receipts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<string>(type: "text", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')"),
                    is_posted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    posted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    posted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_rejected = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    rejected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rejected_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receipts", x => x.id);
                    table.ForeignKey(
                        name: "FK_Receipts_Customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "Customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Receipts_Organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Receipts_Users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Receipts_Users_posted_by_user_id",
                        column: x => x.posted_by_user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Receipts_Users_rejected_by_user_id",
                        column: x => x.rejected_by_user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Receipts_WalletTransactions_wallet_transaction_id",
                        column: x => x.wallet_transaction_id,
                        principalTable: "WalletTransactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    PurchaseReceiptId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    QuantityOrdered = table.Column<int>(type: "integer", nullable: false),
                    QuantityReceived = table.Column<int>(type: "integer", nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    GLInventoryAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseItems_ChartOfAccounts_GLInventoryAccountId",
                        column: x => x.GLInventoryAccountId,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_PurchaseItems_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_PurchaseItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseItems_PurchaseReceipts_PurchaseReceiptId",
                        column: x => x.PurchaseReceiptId,
                        principalTable: "PurchaseReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseReceiptId = table.Column<Guid>(type: "uuid", nullable: true),
                    QuantityRemaining = table.Column<int>(type: "integer", nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric", nullable: false),
                    ReceiptDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockBatches_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_StockBatches_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_StockBatches_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockBatches_PurchaseReceipts_PurchaseReceiptId",
                        column: x => x.PurchaseReceiptId,
                        principalTable: "PurchaseReceipts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VendorInvoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VendorInvoiceReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PurchaseReceiptId = table.Column<Guid>(type: "uuid", nullable: true),
                    VendorId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PaymentStatus = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorInvoices_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VendorInvoices_PurchaseReceipts_PurchaseReceiptId",
                        column: x => x.PurchaseReceiptId,
                        principalTable: "PurchaseReceipts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VendorInvoices_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "ReceiptItems",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    receipt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptItems", x => x.id);
                    table.ForeignKey(
                        name: "FK_ReceiptItems_Receipts_receipt_id",
                        column: x => x.receipt_id,
                        principalTable: "Receipts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_applications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vendor_invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount_applied = table.Column<decimal>(type: "numeric", nullable: false),
                    DiscountTaken = table.Column<decimal>(type: "numeric", nullable: false),
                    ApplicationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() at time zone 'utc')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_applications", x => x.id);
                    table.ForeignKey(
                        name: "FK_payment_applications_VendorInvoices_vendor_invoice_id",
                        column: x => x.vendor_invoice_id,
                        principalTable: "VendorInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payment_applications_payments_payment_id",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalWorkflows_ApproverId",
                table: "ApprovalWorkflows",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalWorkflows_OrganizationId",
                table: "ApprovalWorkflows",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalWorkflows_RequesterId",
                table: "ApprovalWorkflows",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_organization_id",
                table: "AuditLog",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_target_table_target_record_id",
                table: "AuditLog",
                columns: new[] { "target_table", "target_record_id" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_user_id",
                table: "AuditLog",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_organization_id_name",
                table: "Categories",
                columns: new[] { "organization_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_parent_id",
                table: "Categories",
                column: "parent_id");

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
                name: "IX_ChartOfAccounts_ParentAccountId",
                table: "ChartOfAccounts",
                column: "ParentAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_company_bank_accounts_gl_account_id",
                table: "company_bank_accounts",
                column: "gl_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_company_bank_accounts_organization_id",
                table: "company_bank_accounts",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_organization_id_email",
                table: "Contacts",
                columns: new[] { "organization_id", "email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_organization_id_phone",
                table: "Contacts",
                columns: new[] { "organization_id", "phone" },
                unique: true,
                filter: "\"phone\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_assigned_seller_id",
                table: "Customers",
                column: "assigned_seller_id");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_contact_id",
                table: "Customers",
                column: "contact_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_organization_id",
                table: "Customers",
                column: "organization_id");

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
                name: "IX_InventoryAudits_LocationId",
                table: "InventoryAudits",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAudits_organization_id",
                table: "InventoryAudits",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAudits_status",
                table: "InventoryAudits",
                column: "status");

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

            migrationBuilder.CreateIndex(
                name: "IX_Locations_organization_id_name",
                table: "Locations",
                columns: new[] { "organization_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_no_serie_lines_NoSerieId",
                table: "no_serie_lines",
                column: "NoSerieId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_user_id",
                table: "Notifications",
                column: "user_id");

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

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationMembers_organization_id_user_id",
                table: "OrganizationMembers",
                columns: new[] { "organization_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationMembers_user_id",
                table: "OrganizationMembers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_slug",
                table: "Organizations",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_applications_payment_id",
                table: "payment_applications",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_applications_vendor_invoice_id",
                table: "payment_applications",
                column: "vendor_invoice_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_company_bank_account_id",
                table: "payments",
                column: "company_bank_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_created_by_user_id",
                table: "payments",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_organization_id",
                table: "payments",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_payment_journal_entry_id",
                table: "payments",
                column: "payment_journal_entry_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_vendor_bank_account_id",
                table: "payments",
                column: "vendor_bank_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_vendor_id",
                table: "payments",
                column: "vendor_id");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTerms_OrganizationId",
                table: "PaymentTerms",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_category_id",
                table: "Products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_Products_LocationId",
                table: "Products",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_organization_id",
                table: "Products",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTags_tag_id",
                table: "ProductTags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_organization_id_sku",
                table: "ProductVariants",
                columns: new[] { "organization_id", "sku" },
                unique: true,
                filter: "\"sku\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_product_id",
                table: "ProductVariants",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVendors_ProductId",
                table: "ProductVendors",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVendors_VendorId",
                table: "ProductVendors",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItems_GLInventoryAccountId",
                table: "PurchaseItems",
                column: "GLInventoryAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItems_ProductId",
                table: "PurchaseItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItems_ProductVariantId",
                table: "PurchaseItems",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItems_PurchaseReceiptId",
                table: "PurchaseItems",
                column: "PurchaseReceiptId");

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

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_created_by",
                table: "PurchaseOrders",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_organization_id",
                table: "PurchaseOrders",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_organization_id_po_number",
                table: "PurchaseOrders",
                columns: new[] { "organization_id", "po_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_payment_terms_id",
                table: "PurchaseOrders",
                column: "payment_terms_id");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_vendor_id",
                table: "PurchaseOrders",
                column: "vendor_id");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceipts_OrganizationId",
                table: "PurchaseReceipts",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceipts_PurchaseOrderId",
                table: "PurchaseReceipts",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceipts_ReceivedBy",
                table: "PurchaseReceipts",
                column: "ReceivedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceipts_ReceivingJournalEntryId",
                table: "PurchaseReceipts",
                column: "ReceivingJournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceipts_VendorId",
                table: "PurchaseReceipts",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceipts_WarehouseLocationId",
                table: "PurchaseReceipts",
                column: "WarehouseLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptItems_receipt_id",
                table: "ReceiptItems",
                column: "receipt_id");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_created_by_user_id",
                table: "Receipts",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_customer_id",
                table: "Receipts",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_organization_id",
                table: "Receipts",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_posted_by_user_id",
                table: "Receipts",
                column: "posted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_rejected_by_user_id",
                table: "Receipts",
                column: "rejected_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_wallet_transaction_id",
                table: "Receipts",
                column: "wallet_transaction_id",
                unique: true,
                filter: "\"wallet_transaction_id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderItems_organization_id",
                table: "SalesOrderItems",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderItems_product_variant_id",
                table: "SalesOrderItems",
                column: "product_variant_id");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderItems_sales_order_id",
                table: "SalesOrderItems",
                column: "sales_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderItems_tax_rate_id",
                table: "SalesOrderItems",
                column: "tax_rate_id");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_created_by_user_id",
                table: "SalesOrders",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_customer_id",
                table: "SalesOrders",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_organization_id",
                table: "SalesOrders",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_StockBatches_LocationId",
                table: "StockBatches",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockBatches_ProductId",
                table: "StockBatches",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockBatches_ProductVariantId",
                table: "StockBatches",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_StockBatches_PurchaseReceiptId",
                table: "StockBatches",
                column: "PurchaseReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_created_at",
                table: "StockMovements",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_created_by_user_id",
                table: "StockMovements",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_destination_location_id",
                table: "StockMovements",
                column: "destination_location_id");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_organization_id",
                table: "StockMovements",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_posted_by_user_id",
                table: "StockMovements",
                column: "posted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_product_variant_id",
                table: "StockMovements",
                column: "product_variant_id");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_rejected_by_user_id",
                table: "StockMovements",
                column: "rejected_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_related_sales_order_id",
                table: "StockMovements",
                column: "related_sales_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_source_location_id",
                table: "StockMovements",
                column: "source_location_id");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_organization_id_name",
                table: "Tags",
                columns: new[] { "organization_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tax_rates_organization_id",
                table: "tax_rates",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_tax_rates_organization_id_is_default",
                table: "tax_rates",
                columns: new[] { "organization_id", "is_default" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_email",
                table: "Users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vendor_bank_accounts_vendor_id",
                table: "vendor_bank_accounts",
                column: "vendor_id");

            migrationBuilder.CreateIndex(
                name: "IX_VendorInvoices_OrganizationId",
                table: "VendorInvoices",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorInvoices_PurchaseReceiptId",
                table: "VendorInvoices",
                column: "PurchaseReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorInvoices_VendorId",
                table: "VendorInvoices",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_assigned_buyer_id",
                table: "Vendors",
                column: "assigned_buyer_id");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_contact_id",
                table: "Vendors",
                column: "contact_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_organization_id_vendor_code",
                table: "Vendors",
                columns: new[] { "organization_id", "vendor_code" },
                unique: true,
                filter: "\"vendor_code\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_customer_id",
                table: "Wallets",
                column: "customer_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_organization_id",
                table: "Wallets",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_authorized_by_user_id",
                table: "WalletTransactions",
                column: "authorized_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_organization_id",
                table: "WalletTransactions",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_posted_by_user_id",
                table: "WalletTransactions",
                column: "posted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_related_sales_order_id",
                table: "WalletTransactions",
                column: "related_sales_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_wallet_id",
                table: "WalletTransactions",
                column: "wallet_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalWorkflows");

            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "InventoryAuditItems");

            migrationBuilder.DropTable(
                name: "JournalEntryLines");

            migrationBuilder.DropTable(
                name: "no_serie_lines");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OrganizationAccountConfigurations");

            migrationBuilder.DropTable(
                name: "OrganizationMembers");

            migrationBuilder.DropTable(
                name: "payment_applications");

            migrationBuilder.DropTable(
                name: "ProductTags");

            migrationBuilder.DropTable(
                name: "ProductVendors");

            migrationBuilder.DropTable(
                name: "PurchaseItems");

            migrationBuilder.DropTable(
                name: "PurchaseOrderItems");

            migrationBuilder.DropTable(
                name: "ReceiptItems");

            migrationBuilder.DropTable(
                name: "SalesOrderItems");

            migrationBuilder.DropTable(
                name: "StockBatches");

            migrationBuilder.DropTable(
                name: "InventoryAudits");

            migrationBuilder.DropTable(
                name: "StockMovements");

            migrationBuilder.DropTable(
                name: "no_series");

            migrationBuilder.DropTable(
                name: "VendorInvoices");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Receipts");

            migrationBuilder.DropTable(
                name: "tax_rates");

            migrationBuilder.DropTable(
                name: "ProductVariants");

            migrationBuilder.DropTable(
                name: "PurchaseReceipts");

            migrationBuilder.DropTable(
                name: "company_bank_accounts");

            migrationBuilder.DropTable(
                name: "vendor_bank_accounts");

            migrationBuilder.DropTable(
                name: "WalletTransactions");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "JournalEntries");

            migrationBuilder.DropTable(
                name: "PurchaseOrders");

            migrationBuilder.DropTable(
                name: "ChartOfAccounts");

            migrationBuilder.DropTable(
                name: "SalesOrders");

            migrationBuilder.DropTable(
                name: "Wallets");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "PaymentTerms");

            migrationBuilder.DropTable(
                name: "Vendors");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Organizations");
        }
    }
}
