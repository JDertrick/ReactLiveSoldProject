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
