// ===== ENUMS =====
export enum PurchaseOrderStatus {
  Draft = 'Draft',
  Submitted = 'Submitted',
  Approved = 'Approved',
  PartiallyReceived = 'PartiallyReceived',
  Received = 'Received',
  Cancelled = 'Cancelled'
}

export enum PurchaseReceiptStatus {
  Draft = 'Draft',
  Pending = 'Pending',
  Received = 'Received',
  Posted = 'Posted',
  Cancelled = 'Cancelled'
}

export enum InvoiceStatus {
  Pending = 'Pending',
  Approved = 'Approved',
  Paid = 'Paid',
  Cancelled = 'Cancelled'
}

export enum InvoicePaymentStatus {
  Unpaid = 'Unpaid',
  PartiallyPaid = 'PartiallyPaid',
  Paid = 'Paid',
  Overdue = 'Overdue'
}

export enum PaymentStatus {
  Pending = 'Pending',
  Approved = 'Approved',
  Rejected = 'Rejected',
  Posted = 'Posted'
}

export enum PaymentMethod {
  Cash = 'Cash',
  Check = 'Check',
  BankTransfer = 'BankTransfer',
  CreditCard = 'CreditCard',
  DebitCard = 'DebitCard',
  Other = 'Other'
}

// ===== PURCHASE ORDER =====
export interface PurchaseOrderDto {
  id: string;
  organizationId: string;
  poNumber: string;
  vendorId: string;
  vendorName?: string;
  orderDate: string;
  expectedDeliveryDate?: string;
  status: PurchaseOrderStatus;
  subtotal: number;
  taxAmount: number;
  totalAmount: number;
  currency: string;
  exchangeRate: number;
  paymentTermsId?: string;
  paymentTermsDescription?: string;
  notes?: string;
  createdBy: string;
  createdByName?: string;
  createdAt: string;
  updatedAt: string;
  items?: PurchaseOrderItemDto[];
}

export interface PurchaseOrderItemDto {
  id: string;
  purchaseOrderId: string;
  lineNumber: number;
  productId: string;
  productName?: string;
  productVariantId?: string;
  variantName?: string;
  description?: string;
  quantity: number;
  unitCost: number;
  discountPercentage: number;
  discountAmount: number;
  taxRate: number;
  taxAmount: number;
  lineTotal: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreatePurchaseOrderDto {
  vendorId: string;
  orderDate: string;
  expectedDeliveryDate?: string;
  currency?: string;
  exchangeRate?: number;
  paymentTermsId?: string;
  notes?: string;
  items: CreatePurchaseOrderItemDto[];
}

export interface CreatePurchaseOrderItemDto {
  productId: string;
  productVariantId?: string;
  description?: string;
  quantity: number;
  unitCost: number;
  discountPercentage?: number;
  taxRate?: number;
}

// ===== PURCHASE RECEIPT =====
export interface PurchaseReceiptDto {
  id: string;
  organizationId: string;
  receiptNumber: string;
  purchaseOrderId?: string;
  purchaseOrderNumber?: string;
  vendorId: string;
  vendorName?: string;
  receiptDate: string;
  status: PurchaseReceiptStatus;
  warehouseLocationId?: string;
  warehouseLocationName?: string;
  receivedBy: string;
  receivedByName?: string;
  notes?: string;
  receivingJournalEntryId?: string;
  createdAt: string;
  updatedAt: string;
  items?: PurchaseItemDto[];
}

export interface PurchaseItemDto {
  id: string;
  lineNumber: number;
  productId: string;
  productName?: string;
  productVariantId?: string;
  variantName?: string;
  description?: string;
  quantityOrdered: number;
  quantityReceived: number;
  unitCost: number;
  discountPercentage: number;
  taxRate: number;
  taxAmount: number;
  lineTotal: number;
}

export interface CreatePurchaseReceiptDto {
  purchaseOrderId?: string;
  vendorId: string;
  receiptDate: string;
  warehouseLocationId?: string;
  notes?: string;
  items: CreatePurchaseItemDto[];
}

export interface CreatePurchaseItemDto {
  lineNumber: number;
  productId: string;
  productVariantId?: string;
  description?: string;
  quantityReceived: number;
  unitCost: number;
  discountPercentage?: number;
  taxRate?: number;
  glInventoryAccountId?: string;
}

export interface ReceivePurchaseDto {
  purchaseReceiptId: string;
  defaultGLInventoryAccountId?: string;
  defaultGLAccountsPayableId?: string;
  defaultGLTaxAccountId?: string;
}

// ===== VENDOR INVOICE =====
export interface VendorInvoiceDto {
  id: string;
  organizationId: string;
  invoiceNumber: string;
  vendorInvoiceReference?: string;
  purchaseReceiptId?: string;
  purchaseReceiptNumber?: string;
  vendorId: string;
  vendorName?: string;
  invoiceDate: string;
  dueDate: string;
  subtotal: number;
  taxAmount: number;
  totalAmount: number;
  amountPaid: number;
  amountDue: number;
  status: InvoiceStatus;
  paymentStatus: InvoicePaymentStatus;
  notes?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateVendorInvoiceDto {
  vendorInvoiceReference?: string;
  purchaseReceiptId?: string;
  vendorId: string;
  invoiceDate: string;
  dueDate: string;
  subtotal: number;
  taxAmount: number;
  totalAmount: number;
  notes?: string;
}

// ===== PAYMENT =====
export interface PaymentDto {
  id: string;
  organizationId: string;
  paymentNumber: string;
  vendorId: string;
  vendorName?: string;
  paymentDate: string;
  paymentMethod: PaymentMethod;
  referenceNumber?: string;
  amount: number;
  currency: string;
  exchangeRate: number;
  companyBankAccountId?: string;
  companyBankAccountName?: string;
  vendorBankAccountId?: string;
  notes?: string;
  status: PaymentStatus;
  approvedBy?: string;
  approvedByName?: string;
  approvedAt?: string;
  rejectedBy?: string;
  rejectedByName?: string;
  rejectedAt?: string;
  rejectionReason?: string;
  postedBy?: string;
  postedByName?: string;
  postedAt?: string;
  paymentJournalEntryId?: string;
  createdBy: string;
  createdByName?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreatePaymentDto {
  vendorId: string;
  paymentDate: string;
  paymentMethod: PaymentMethod;
  referenceNumber?: string;
  amount: number;
  currency?: string;
  exchangeRate?: number;
  companyBankAccountId?: string;
  vendorBankAccountId?: string;
  notes?: string;
  invoiceApplications: PaymentInvoiceApplicationDto[];
}

export interface PaymentInvoiceApplicationDto {
  vendorInvoiceId: string;
  amountApplied: number;
}

// ===== PAYMENT TERMS =====
export interface PaymentTermsDto {
  id: string;
  organizationId: string;
  description: string;
  dueDays: number;
  discountPercentage: number;
  discountDays: number;
  isActive: boolean;
}

export interface CreatePaymentTermsDto {
  description: string;
  dueDays: number;
  discountPercentage?: number;
  discountDays?: number;
  isActive?: boolean;
}

export interface UpdatePaymentTermsDto {
  description?: string;
  dueDays?: number;
  discountPercentage?: number;
  discountDays?: number;
  isActive?: boolean;
}

// ===== BANK ACCOUNTS =====
export interface CompanyBankAccountDto {
  id: string;
  organizationId: string;
  bankName: string;
  accountNumber: string;
  currency: string;
  currentBalance: number;
  glAccountId: string;
  glAccountName?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCompanyBankAccountDto {
  bankName: string;
  accountNumber: string;
  currency: string;
  currentBalance: number;
  glAccountId: string;
}

export interface UpdateCompanyBankAccountDto {
  bankName?: string;
  accountNumber?: string;
  currency?: string;
  isActive?: boolean;
}

export interface VendorBankAccountDto {
  id: string;
  vendorId: string;
  bankName: string;
  accountNumber: string;
  accountHolderName: string;
  clabe_IBAN?: string;
  accountType?: string;
  isPrimary: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateVendorBankAccountDto {
  vendorId: string;
  bankName: string;
  accountNumber: string;
  accountHolderName: string;
  clabe_IBAN?: string;
  accountType?: string;
  isPrimary?: boolean;
  isActive?: boolean;
}

export interface UpdateVendorBankAccountDto {
  bankName?: string;
  accountNumber?: string;
  accountHolderName?: string;
  clabe_IBAN?: string;
  accountType?: string;
  isPrimary?: boolean;
  isActive?: boolean;
}

// ===== PRODUCT VENDOR =====
export interface ProductVendorDto {
  id: string;
  productId: string;
  productName?: string;
  vendorId: string;
  vendorName?: string;
  vendorSKU?: string;
  costPrice: number;
  leadTimeDays: number;
  minimumOrderQuantity: number;
  isPreferred: boolean;
  validFrom: string;
  validTo?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateProductVendorDto {
  productId: string;
  vendorId: string;
  vendorSKU?: string;
  costPrice: number;
  leadTimeDays?: number;
  minimumOrderQuantity?: number;
  isPreferred?: boolean;
  validFrom?: string;
  validTo?: string;
  isActive?: boolean;
}

export interface UpdateProductVendorDto {
  vendorSKU?: string;
  costPrice?: number;
  leadTimeDays?: number;
  minimumOrderQuantity?: number;
  isPreferred?: boolean;
  validFrom?: string;
  validTo?: string;
  isActive?: boolean;
}
