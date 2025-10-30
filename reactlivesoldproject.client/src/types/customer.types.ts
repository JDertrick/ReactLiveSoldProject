// Customer Types
export interface Customer {
  id: string;
  organizationId: string;
  firstName?: string;
  lastName?: string;
  email: string;
  phone?: string;
  assignedSellerId?: string;
  assignedSellerName?: string;
  notes?: string;
  walletBalance: number;
  createdAt: string;
  updatedat: string;
}

export interface CreateCustomerDto {
  email: string;
  password: string;
  firstName?: string;
  lastName?: string;
  phone?: string;
  assignedSellerId?: string;
  notes?: string;
}

export interface UpdateCustomerDto {
  email: string;
  password?: string;
  firstName?: string;
  lastName?: string;
  phone?: string;
  assignedSellerId?: string;
  notes?: string;
}
