export enum StockMovementType {
  InitialStock = 'InitialStock',
  Purchase = 'Purchase',
  Sale = 'Sale',
  Return = 'Return',
  Adjustment = 'Adjustment',
  Loss = 'Loss',
  Transfer = 'Transfer',
  SaleCancellation = 'SaleCancellation'
}

export interface StockMovementDto {
  id: string;
  productVariantId: string;
  productName: string;
  variantSku: string;
  movementType: string;
  quantity: number;
  stockBefore: number;
  stockAfter: number;
  relatedSalesOrderId?: string;
  createdByUserName?: string;
  notes?: string;
  reference?: string;
  unitCost?: number;
  isPosted: boolean;
  postedAt?: string;
  postedByUserName?: string;
  isRejected: boolean;
  rejectedAt?: string;
  rejectedByUserName?: string;
  createdAt: string;
}

export interface CreateStockMovementDto {
  productVariantId: string;
  movementType: string;
  quantity: number;
  notes?: string;
  reference?: string;
  unitCost?: number;
}

export interface StockMovementFilters {
  productVariantId?: string;
  fromDate?: string;
  toDate?: string;
}
