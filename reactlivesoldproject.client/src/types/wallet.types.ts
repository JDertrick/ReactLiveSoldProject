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
  receiptId?: string; // New property to link to a Receipt
  authorizedByUserId?: string;
  authorizedByUserName?: string;
  notes?: string;
  reference?: string;
  isPosted: boolean;
  balanceBefore?: number;
  balanceAfter?: number;
  postedByUserId?: string;
  postedByUserName?: string;
  postedAt?: string;
  createdAt: string;
}

export interface CreateWalletTransactionDto {
  customerId: string;
  type: string;
  amount: number;
  reference?: string;
  notes?: string;
}

// New Receipt Types
export interface ReceiptItem {
  id: string;
  description: string;
  unitPrice: number;
  quantity: number;
  subtotal: number;
}

export interface ReceiptItemDto {
  id: string;
  description: string;
  unitPrice: number;
  quantity: number;
  subtotal: number;
}

export interface Receipt {
  id: string;
  organizationId: string;
  customerId: string;
  customerName: string;
  walletTransactionId?: string; // Nullable
  type: 'Deposit' | 'Withdrawal';
  totalAmount: number;
  notes?: string;
  createdByUserId: string;
  createdByUserName: string;
  createdAt: string;
  isPosted: boolean;
  postedAt?: string;
  postedByUserName?: string;
  isRejected: boolean;
  rejectedAt?: string;
  rejectedByUserName?: string;
  items: ReceiptItem[];
}

export interface CreateReceiptItemDto {
  description: string;
  unitPrice: number;
  quantity: number;
}

export interface CreateReceiptDto {
  customerId: string;
  type: 'Deposit' | 'Withdrawal';
  notes?: string;
  items: CreateReceiptItemDto[];
}

export interface ReceiptDto {
  id: string;
  organizationId: string;
  customerId: string;
  customerName: string;
  walletTransactionId?: string; // Nullable
  type: 'Deposit' | 'Withdrawal';
  totalAmount: number;
  notes?: string;
  createdAt: string;
  isPosted: boolean;
  postedAt?: string;
  postedByUserName?: string;
  isRejected: boolean;
  rejectedAt?: string;
  rejectedByUserName?: string;
  items: ReceiptItemDto[];
}
