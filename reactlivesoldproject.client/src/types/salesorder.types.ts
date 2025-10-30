// Sales Order Types
export interface SalesOrderItem {
  id: string;
  productVariantId: string;
  productName: string;
  variantSku?: string;
  variantAttributes?: string;
  quantity: number;
  originalPrice: number;
  unitPrice: number;
  subtotal: number;
  itemDescription?: string;
}

export interface SalesOrder {
  id: string;
  customerId: string;
  customerName: string;
  customerEmail: string;
  createdByUserId?: string;
  createdByUserName?: string;
  status: string;
  totalAmount: number;
  notes?: string;
  items: SalesOrderItem[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateSalesOrderItemDto {
  productVariantId: string;
  quantity: number;
  customUnitPrice?: number;
  itemDescription?: string;
}

export interface CreateSalesOrderDto {
  customerId: string;
  notes?: string;
  items: CreateSalesOrderItemDto[];
}
