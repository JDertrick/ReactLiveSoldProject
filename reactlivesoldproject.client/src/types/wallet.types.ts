// Wallet Types
export interface Wallet {
  id: string;
  customerId: string;
  customerName: string;
  customerEmail: string;
  balance: number;
  updatedAt: string;
}

export interface WalletTransaction {
  id: string;
  walletId: string;
  type: string;
  amount: number;
  relatedSalesOrderId?: string;
  authorizedByUserId?: string;
  authorizedByUserName?: string;
  notes?: string;
  createdAt: string;
}

export interface CreateWalletTransactionDto {
  customerId: string;
  type: string;
  amount: number;
  notes?: string;
}
