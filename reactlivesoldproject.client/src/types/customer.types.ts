import { Wallet } from "./wallet.types";

// Customer Types
export interface Customer {
  id: string;
  organizationId: string;
  firstName?: string;
  lastName?: string;
  email: string;
  phone?: string;
  phoneNumber?: string; // Alias for phone
  assignedSellerId?: string;
  assignedSellerName?: string;
  notes?: string;
  walletBalance: number;
  createdAt: string;
  updatedat: string;
  isActive: boolean;
  wallet: Wallet;
}

export interface CreateCustomerDto {
  email: string;
  password: string;
  firstName?: string;
  lastName?: string;
  phone?: string;
  dateOfBirth?: string;
  shippingAddress?: string;
  assignedSellerId?: string;
  notes?: string;
  isActive: boolean
}

export interface UpdateCustomerDto {
  email: string;
  password?: string;
  firstName?: string;
  lastName?: string;
  phone?: string;
  assignedSellerId?: string;
  notes?: string;
  isActive: boolean;
}
