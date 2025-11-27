using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReactLiveSoldProject.ServerBL.Migrations
{
    /// <inheritdoc />
    public partial class _6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyBankAccounts_ChartOfAccounts_GLAccountId",
                table: "CompanyBankAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanyBankAccounts_Organizations_OrganizationId",
                table: "CompanyBankAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentApplications_Payments_PaymentId",
                table: "PaymentApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentApplications_VendorInvoices_VendorInvoiceId",
                table: "PaymentApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_CompanyBankAccounts_CompanyBankAccountId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_JournalEntries_PaymentJournalEntryId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Organizations_OrganizationId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Users_CreatedByUserId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_VendorBankAccounts_VendorBankAccountId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Vendors_VendorId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_VendorBankAccounts_Vendors_VendorId",
                table: "VendorBankAccounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payments",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_CreatedByUserId",
                table: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VendorBankAccounts",
                table: "VendorBankAccounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentApplications",
                table: "PaymentApplications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanyBankAccounts",
                table: "CompanyBankAccounts");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Payments");

            migrationBuilder.RenameTable(
                name: "Payments",
                newName: "payments");

            migrationBuilder.RenameTable(
                name: "VendorBankAccounts",
                newName: "vendor_bank_accounts");

            migrationBuilder.RenameTable(
                name: "PaymentApplications",
                newName: "payment_applications");

            migrationBuilder.RenameTable(
                name: "CompanyBankAccounts",
                newName: "company_bank_accounts");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "payments",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "payments",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "Currency",
                table: "payments",
                newName: "currency");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "payments",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "VendorId",
                table: "payments",
                newName: "vendor_id");

            migrationBuilder.RenameColumn(
                name: "VendorBankAccountId",
                table: "payments",
                newName: "vendor_bank_account_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "payments",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "ReferenceNumber",
                table: "payments",
                newName: "reference_number");

            migrationBuilder.RenameColumn(
                name: "PaymentNumber",
                table: "payments",
                newName: "payment_number");

            migrationBuilder.RenameColumn(
                name: "PaymentMethod",
                table: "payments",
                newName: "payment_method");

            migrationBuilder.RenameColumn(
                name: "PaymentJournalEntryId",
                table: "payments",
                newName: "payment_journal_entry_id");

            migrationBuilder.RenameColumn(
                name: "PaymentDate",
                table: "payments",
                newName: "payment_date");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "payments",
                newName: "organization_id");

            migrationBuilder.RenameColumn(
                name: "ExchangeRate",
                table: "payments",
                newName: "exchange_rate");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "payments",
                newName: "created_by_user_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "payments",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CompanyBankAccountId",
                table: "payments",
                newName: "company_bank_account_id");

            migrationBuilder.RenameColumn(
                name: "AmountPaid",
                table: "payments",
                newName: "amount_paid");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_VendorId",
                table: "payments",
                newName: "IX_payments_vendor_id");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_VendorBankAccountId",
                table: "payments",
                newName: "IX_payments_vendor_bank_account_id");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_PaymentJournalEntryId",
                table: "payments",
                newName: "IX_payments_payment_journal_entry_id");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_OrganizationId",
                table: "payments",
                newName: "IX_payments_organization_id");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_CompanyBankAccountId",
                table: "payments",
                newName: "IX_payments_company_bank_account_id");

            migrationBuilder.RenameColumn(
                name: "CLABE_IBAN",
                table: "vendor_bank_accounts",
                newName: "clabe_iban");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "vendor_bank_accounts",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "VendorId",
                table: "vendor_bank_accounts",
                newName: "vendor_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "vendor_bank_accounts",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "IsPrimary",
                table: "vendor_bank_accounts",
                newName: "is_primary");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "vendor_bank_accounts",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "vendor_bank_accounts",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "BankName",
                table: "vendor_bank_accounts",
                newName: "bank_name");

            migrationBuilder.RenameColumn(
                name: "AccountType",
                table: "vendor_bank_accounts",
                newName: "account_type");

            migrationBuilder.RenameColumn(
                name: "AccountNumber",
                table: "vendor_bank_accounts",
                newName: "account_number");

            migrationBuilder.RenameColumn(
                name: "AccountHolderName",
                table: "vendor_bank_accounts",
                newName: "account_holder_name");

            migrationBuilder.RenameIndex(
                name: "IX_VendorBankAccounts_VendorId",
                table: "vendor_bank_accounts",
                newName: "IX_vendor_bank_accounts_vendor_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "payment_applications",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "VendorInvoiceId",
                table: "payment_applications",
                newName: "vendor_invoice_id");

            migrationBuilder.RenameColumn(
                name: "PaymentId",
                table: "payment_applications",
                newName: "payment_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "payment_applications",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "AmountApplied",
                table: "payment_applications",
                newName: "amount_applied");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentApplications_VendorInvoiceId",
                table: "payment_applications",
                newName: "IX_payment_applications_vendor_invoice_id");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentApplications_PaymentId",
                table: "payment_applications",
                newName: "IX_payment_applications_payment_id");

            migrationBuilder.RenameColumn(
                name: "Currency",
                table: "company_bank_accounts",
                newName: "currency");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "company_bank_accounts",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "company_bank_accounts",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "company_bank_accounts",
                newName: "organization_id");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "company_bank_accounts",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "GLAccountId",
                table: "company_bank_accounts",
                newName: "gl_account_id");

            migrationBuilder.RenameColumn(
                name: "CurrentBalance",
                table: "company_bank_accounts",
                newName: "current_balance");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "company_bank_accounts",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "BankName",
                table: "company_bank_accounts",
                newName: "bank_name");

            migrationBuilder.RenameColumn(
                name: "AccountNumber",
                table: "company_bank_accounts",
                newName: "account_number");

            migrationBuilder.RenameIndex(
                name: "IX_CompanyBankAccounts_OrganizationId",
                table: "company_bank_accounts",
                newName: "IX_company_bank_accounts_organization_id");

            migrationBuilder.RenameIndex(
                name: "IX_CompanyBankAccounts_GLAccountId",
                table: "company_bank_accounts",
                newName: "IX_company_bank_accounts_gl_account_id");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "payments",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "payment_method",
                table: "payments",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddPrimaryKey(
                name: "PK_payments",
                table: "payments",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_vendor_bank_accounts",
                table: "vendor_bank_accounts",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_payment_applications",
                table: "payment_applications",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_company_bank_accounts",
                table: "company_bank_accounts",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_created_by_user_id",
                table: "payments",
                column: "created_by_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_company_bank_accounts_ChartOfAccounts_gl_account_id",
                table: "company_bank_accounts",
                column: "gl_account_id",
                principalTable: "ChartOfAccounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_company_bank_accounts_Organizations_organization_id",
                table: "company_bank_accounts",
                column: "organization_id",
                principalTable: "Organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_payment_applications_VendorInvoices_vendor_invoice_id",
                table: "payment_applications",
                column: "vendor_invoice_id",
                principalTable: "VendorInvoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payment_applications_payments_payment_id",
                table: "payment_applications",
                column: "payment_id",
                principalTable: "payments",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Users_CreatedByUserId",
                table: "payments",
                column: "created_by_user_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payments_JournalEntries_payment_journal_entry_id",
                table: "payments",
                column: "payment_journal_entry_id",
                principalTable: "JournalEntries",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_payments_Organizations_organization_id",
                table: "payments",
                column: "organization_id",
                principalTable: "Organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_payments_Vendors_vendor_id",
                table: "payments",
                column: "vendor_id",
                principalTable: "Vendors",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payments_company_bank_accounts_company_bank_account_id",
                table: "payments",
                column: "company_bank_account_id",
                principalTable: "company_bank_accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payments_vendor_bank_accounts_vendor_bank_account_id",
                table: "payments",
                column: "vendor_bank_account_id",
                principalTable: "vendor_bank_accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_vendor_bank_accounts_Vendors_vendor_id",
                table: "vendor_bank_accounts",
                column: "vendor_id",
                principalTable: "Vendors",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_company_bank_accounts_ChartOfAccounts_gl_account_id",
                table: "company_bank_accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_company_bank_accounts_Organizations_organization_id",
                table: "company_bank_accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_payment_applications_VendorInvoices_vendor_invoice_id",
                table: "payment_applications");

            migrationBuilder.DropForeignKey(
                name: "FK_payment_applications_payments_payment_id",
                table: "payment_applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Users_CreatedByUserId",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "FK_payments_JournalEntries_payment_journal_entry_id",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "FK_payments_Organizations_organization_id",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "FK_payments_Vendors_vendor_id",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "FK_payments_company_bank_accounts_company_bank_account_id",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "FK_payments_vendor_bank_accounts_vendor_bank_account_id",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "FK_vendor_bank_accounts_Vendors_vendor_id",
                table: "vendor_bank_accounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_payments",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "IX_payments_created_by_user_id",
                table: "payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_vendor_bank_accounts",
                table: "vendor_bank_accounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_payment_applications",
                table: "payment_applications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_company_bank_accounts",
                table: "company_bank_accounts");

            migrationBuilder.RenameTable(
                name: "payments",
                newName: "Payments");

            migrationBuilder.RenameTable(
                name: "vendor_bank_accounts",
                newName: "VendorBankAccounts");

            migrationBuilder.RenameTable(
                name: "payment_applications",
                newName: "PaymentApplications");

            migrationBuilder.RenameTable(
                name: "company_bank_accounts",
                newName: "CompanyBankAccounts");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Payments",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "Payments",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "currency",
                table: "Payments",
                newName: "Currency");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Payments",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "vendor_id",
                table: "Payments",
                newName: "VendorId");

            migrationBuilder.RenameColumn(
                name: "vendor_bank_account_id",
                table: "Payments",
                newName: "VendorBankAccountId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Payments",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "reference_number",
                table: "Payments",
                newName: "ReferenceNumber");

            migrationBuilder.RenameColumn(
                name: "payment_number",
                table: "Payments",
                newName: "PaymentNumber");

            migrationBuilder.RenameColumn(
                name: "payment_method",
                table: "Payments",
                newName: "PaymentMethod");

            migrationBuilder.RenameColumn(
                name: "payment_journal_entry_id",
                table: "Payments",
                newName: "PaymentJournalEntryId");

            migrationBuilder.RenameColumn(
                name: "payment_date",
                table: "Payments",
                newName: "PaymentDate");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                table: "Payments",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "exchange_rate",
                table: "Payments",
                newName: "ExchangeRate");

            migrationBuilder.RenameColumn(
                name: "created_by_user_id",
                table: "Payments",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Payments",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "company_bank_account_id",
                table: "Payments",
                newName: "CompanyBankAccountId");

            migrationBuilder.RenameColumn(
                name: "amount_paid",
                table: "Payments",
                newName: "AmountPaid");

            migrationBuilder.RenameIndex(
                name: "IX_payments_vendor_id",
                table: "Payments",
                newName: "IX_Payments_VendorId");

            migrationBuilder.RenameIndex(
                name: "IX_payments_vendor_bank_account_id",
                table: "Payments",
                newName: "IX_Payments_VendorBankAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_payments_payment_journal_entry_id",
                table: "Payments",
                newName: "IX_Payments_PaymentJournalEntryId");

            migrationBuilder.RenameIndex(
                name: "IX_payments_organization_id",
                table: "Payments",
                newName: "IX_Payments_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_payments_company_bank_account_id",
                table: "Payments",
                newName: "IX_Payments_CompanyBankAccountId");

            migrationBuilder.RenameColumn(
                name: "clabe_iban",
                table: "VendorBankAccounts",
                newName: "CLABE_IBAN");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "VendorBankAccounts",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "vendor_id",
                table: "VendorBankAccounts",
                newName: "VendorId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "VendorBankAccounts",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "is_primary",
                table: "VendorBankAccounts",
                newName: "IsPrimary");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "VendorBankAccounts",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "VendorBankAccounts",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "bank_name",
                table: "VendorBankAccounts",
                newName: "BankName");

            migrationBuilder.RenameColumn(
                name: "account_type",
                table: "VendorBankAccounts",
                newName: "AccountType");

            migrationBuilder.RenameColumn(
                name: "account_number",
                table: "VendorBankAccounts",
                newName: "AccountNumber");

            migrationBuilder.RenameColumn(
                name: "account_holder_name",
                table: "VendorBankAccounts",
                newName: "AccountHolderName");

            migrationBuilder.RenameIndex(
                name: "IX_vendor_bank_accounts_vendor_id",
                table: "VendorBankAccounts",
                newName: "IX_VendorBankAccounts_VendorId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "PaymentApplications",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "vendor_invoice_id",
                table: "PaymentApplications",
                newName: "VendorInvoiceId");

            migrationBuilder.RenameColumn(
                name: "payment_id",
                table: "PaymentApplications",
                newName: "PaymentId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "PaymentApplications",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "amount_applied",
                table: "PaymentApplications",
                newName: "AmountApplied");

            migrationBuilder.RenameIndex(
                name: "IX_payment_applications_vendor_invoice_id",
                table: "PaymentApplications",
                newName: "IX_PaymentApplications_VendorInvoiceId");

            migrationBuilder.RenameIndex(
                name: "IX_payment_applications_payment_id",
                table: "PaymentApplications",
                newName: "IX_PaymentApplications_PaymentId");

            migrationBuilder.RenameColumn(
                name: "currency",
                table: "CompanyBankAccounts",
                newName: "Currency");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "CompanyBankAccounts",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "CompanyBankAccounts",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                table: "CompanyBankAccounts",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "CompanyBankAccounts",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "gl_account_id",
                table: "CompanyBankAccounts",
                newName: "GLAccountId");

            migrationBuilder.RenameColumn(
                name: "current_balance",
                table: "CompanyBankAccounts",
                newName: "CurrentBalance");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "CompanyBankAccounts",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "bank_name",
                table: "CompanyBankAccounts",
                newName: "BankName");

            migrationBuilder.RenameColumn(
                name: "account_number",
                table: "CompanyBankAccounts",
                newName: "AccountNumber");

            migrationBuilder.RenameIndex(
                name: "IX_company_bank_accounts_organization_id",
                table: "CompanyBankAccounts",
                newName: "IX_CompanyBankAccounts_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_company_bank_accounts_gl_account_id",
                table: "CompanyBankAccounts",
                newName: "IX_CompanyBankAccounts_GLAccountId");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Payments",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "PaymentMethod",
                table: "Payments",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Payments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payments",
                table: "Payments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VendorBankAccounts",
                table: "VendorBankAccounts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentApplications",
                table: "PaymentApplications",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanyBankAccounts",
                table: "CompanyBankAccounts",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CreatedByUserId",
                table: "Payments",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyBankAccounts_ChartOfAccounts_GLAccountId",
                table: "CompanyBankAccounts",
                column: "GLAccountId",
                principalTable: "ChartOfAccounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyBankAccounts_Organizations_OrganizationId",
                table: "CompanyBankAccounts",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentApplications_Payments_PaymentId",
                table: "PaymentApplications",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentApplications_VendorInvoices_VendorInvoiceId",
                table: "PaymentApplications",
                column: "VendorInvoiceId",
                principalTable: "VendorInvoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_CompanyBankAccounts_CompanyBankAccountId",
                table: "Payments",
                column: "CompanyBankAccountId",
                principalTable: "CompanyBankAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_JournalEntries_PaymentJournalEntryId",
                table: "Payments",
                column: "PaymentJournalEntryId",
                principalTable: "JournalEntries",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Organizations_OrganizationId",
                table: "Payments",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Users_CreatedByUserId",
                table: "Payments",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_VendorBankAccounts_VendorBankAccountId",
                table: "Payments",
                column: "VendorBankAccountId",
                principalTable: "VendorBankAccounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Vendors_VendorId",
                table: "Payments",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VendorBankAccounts_Vendors_VendorId",
                table: "VendorBankAccounts",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
