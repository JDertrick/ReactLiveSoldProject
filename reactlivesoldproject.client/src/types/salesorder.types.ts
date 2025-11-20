// Sales Order Types
export enum SaleType {
  Retail = "Retail",
  Wholesale = "Wholesale"
}

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
  orderNumber?: string;
  items: SalesOrderItem[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateSalesOrderItemDto {
  productVariantId: string;
  quantity: number;
  saleType?: SaleType;
  customUnitPrice?: number;
  itemDescription?: string;
}

export interface CreateSalesOrderDto {
  customerId: string;
  notes?: string;
  items: CreateSalesOrderItemDto[];
}

export interface UpdateSalesOrderItemDto {
  quantity: number;
  customUnitPrice?: number;
  itemDescription?: string;
}
