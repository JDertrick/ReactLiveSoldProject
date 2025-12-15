export enum DocumentType {
  Customer = "Customer",
  Vendor = "Vendor",
  SalesInvoice = "SalesInvoice",
  SalesOrder = "SalesOrder",
  PurchaseInvoice = "PurchaseInvoice",
  PurchaseOrder = "PurchaseOrder",
  PurchaseReceipt = "PurchaseReceipt",
  Product = "Product",
  ProductVariant = "ProductVariant",
  Payment = "Payment",
  JournalEntry = "JournalEntry",
  WalletTransaction = "WalletTransaction",
  StockMovement = "StockMovement",
  Contact = "Contact",
}

export const DocumentTypeLabels: Record<DocumentType, string> = {
  [DocumentType.Customer]: 'Clientes',
  [DocumentType.Vendor]: 'Proveedores',
  [DocumentType.SalesInvoice]: 'Facturas de Venta',
  [DocumentType.SalesOrder]: 'Órdenes de Venta',
  [DocumentType.PurchaseInvoice]: 'Facturas de Compra',
  [DocumentType.PurchaseOrder]: 'Órdenes de Compra',
  [DocumentType.PurchaseReceipt]: 'Recibos de Compra',
  [DocumentType.Product]: 'Productos',
  [DocumentType.ProductVariant]: 'Variantes de Producto',
  [DocumentType.Payment]: 'Pagos',
  [DocumentType.JournalEntry]: 'Asientos Contables',
  [DocumentType.WalletTransaction]: 'Transacciones de Wallet',
  [DocumentType.StockMovement]: 'Movimientos de Inventario',
  [DocumentType.Contact]: 'Contactos',
};

export interface NoSerieLine {
  id: string;
  noSerieId: string;
  startingDate: string;
  startingNo: string;
  endingNo: string;
  lastNoUsed: string;
  incrementBy: number;
  warningNo: string;
  open: boolean;
}

export interface NoSerie {
  id: string;
  organizationId: string;
  code: string;
  description: string;
  documentType?: DocumentType;
  documentTypeName?: string;
  defaultNos: boolean;
  manualNos: boolean;
  dateOrder: boolean;
  createdAt: string;
  updatedAt: string;
  noSerieLines: NoSerieLine[];
}

export interface CreateNoSerieDto {
  code: string;
  description: string;
  documentType: DocumentType; // REQUIRED: El tipo de documento es obligatorio
  defaultNos: boolean;
  manualNos: boolean;
  dateOrder: boolean;
  noSerieLines?: CreateNoSerieLineDto[];
}

export interface UpdateNoSerieDto {
  description?: string;
  documentType?: DocumentType;
  defaultNos?: boolean;
  manualNos?: boolean;
  dateOrder?: boolean;
}

export interface CreateNoSerieLineDto {
  startingDate: string;
  startingNo: string;
  endingNo: string;
  incrementBy: number;
  warningNo?: string;
  open: boolean;
}

export interface UpdateNoSerieLineDto {
  startingDate?: string;
  startingNo?: string;
  endingNo?: string;
  incrementBy?: number;
  warningNo?: string;
  open?: boolean;
}

export interface NextNumberResponse {
  serieCode?: string;
  documentType?: string;
  nextNumber: string;
  date: string;
}

export interface ValidateNumberResponse {
  serieCode: string;
  number: string;
  isValid: boolean;
}
