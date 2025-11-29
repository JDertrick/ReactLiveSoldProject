export interface OrganizationAccountConfigurationDto {
  id: string;
  organizationId: string;
  inventoryAccountId?: string;
  inventoryAccountName?: string;
  accountsPayableAccountId?: string;
  accountsPayableAccountName?: string;
  accountsReceivableAccountId?: string;
  accountsReceivableAccountName?: string;
  salesRevenueAccountId?: string;
  salesRevenueAccountName?: string;
  costOfGoodsSoldAccountId?: string;
  costOfGoodsSoldAccountName?: string;
  taxPayableAccountId?: string;
  taxPayableAccountName?: string;
  taxReceivableAccountId?: string;
  taxReceivableAccountName?: string;
  cashAccountId?: string;
  cashAccountName?: string;
  defaultBankAccountId?: string;
  defaultBankAccountName?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface UpdateOrganizationAccountConfigurationDto {
  inventoryAccountId?: string;
  accountsPayableAccountId?: string;
  accountsReceivableAccountId?: string;
  salesRevenueAccountId?: string;
  costOfGoodsSoldAccountId?: string;
  taxPayableAccountId?: string;
  taxReceivableAccountId?: string;
  cashAccountId?: string;
  defaultBankAccountId?: string;
}

export enum AccountType {
  Asset = "Asset",
  Liability = "Liability",
  Equity = "Equity",
  Revenue = "Revenue",
  Expense = "Expense"
}

export enum SystemAccountType {
  Inventory = "Inventory",
  AccountsPayable = "AccountsPayable",
  AccountsReceivable = "AccountsReceivable",
  SalesRevenue = "SalesRevenue",
  CostOfGoodsSold = "CostOfGoodsSold",
  TaxPayable = "TaxPayable",
  TaxReceivable = "TaxReceivable",
  Cash = "Cash",
  BankAccount = "BankAccount"
}

export interface ChartOfAccountDto {
  id: string;
  accountCode: string;
  accountName: string;
  accountType: AccountType;
  systemAccountType?: SystemAccountType;
  description: string;
  isActive: boolean;
}

export interface CreateChartOfAccountDto {
  accountCode: string;
  accountName: string;
  accountType: AccountType;
  systemAccountType?: SystemAccountType;
  description?: string;
}

export interface JournalEntryLineDto {
  id: string;
  accountId: string;
  accountName: string;
  accountCode: string;
  debit: number;
  credit: number;
}

export interface JournalEntryDto {
  id: string;
  entryDate: string;
  description: string;
  referenceNumber: string;
  lines: JournalEntryLineDto[];
}

export interface CreateJournalEntryLineDto {
  accountId: string;
  debit: number;
  credit: number;
}

export interface CreateJournalEntryDto {
  entryDate: string;
  description: string;
  referenceNumber?: string;
  lines: CreateJournalEntryLineDto[];
}
