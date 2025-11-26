using ReactLiveSoldProject.ServerBL.Base;

namespace ReactLiveSoldProject.ServerBL.DTOs.Purchases
{
    // ===== PURCHASE ORDER =====
    public class PurchaseOrderDto
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string PONumber { get; set; }
        public Guid VendorId { get; set; }
        public string? VendorName { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public PurchaseOrderStatus Status { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; }
        public decimal ExchangeRate { get; set; }
        public Guid? PaymentTermsId { get; set; }
        public string? PaymentTermsDescription { get; set; }
        public string? Notes { get; set; }
        public Guid CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<PurchaseOrderItemDto>? Items { get; set; }
    }

    public class CreatePurchaseOrderDto
    {
        public Guid VendorId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string Currency { get; set; } = "MXN";
        public decimal ExchangeRate { get; set; } = 1.0m;
        public Guid? PaymentTermsId { get; set; }
        public string? Notes { get; set; }
        public List<CreatePurchaseOrderItemDto> Items { get; set; } = new();
    }

    public class CreatePurchaseOrderItemDto
    {
        public Guid ProductId { get; set; }
        public Guid? ProductVariantId { get; set; }
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal DiscountPercentage { get; set; } = 0;
        public decimal TaxRate { get; set; } = 0;
    }

    public class PurchaseOrderItemDto
    {
        public Guid Id { get; set; }
        public Guid PurchaseOrderId { get; set; }
        public int LineNumber { get; set; }
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public Guid? ProductVariantId { get; set; }
        public string? VariantName { get; set; }
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // ===== PURCHASE RECEIPT =====
    public class PurchaseReceiptDto
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string ReceiptNumber { get; set; }
        public Guid? PurchaseOrderId { get; set; }
        public string? PurchaseOrderNumber { get; set; }
        public Guid VendorId { get; set; }
        public string? VendorName { get; set; }
        public DateTime ReceiptDate { get; set; }
        public PurchaseReceiptStatus Status { get; set; }
        public Guid? WarehouseLocationId { get; set; }
        public string? WarehouseLocationName { get; set; }
        public Guid ReceivedBy { get; set; }
        public string? ReceivedByName { get; set; }
        public string? Notes { get; set; }
        public Guid? ReceivingJournalEntryId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<PurchaseItemDto>? Items { get; set; }
    }

    public class CreatePurchaseReceiptDto
    {
        public Guid? PurchaseOrderId { get; set; }
        public Guid VendorId { get; set; }
        public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;
        public Guid? WarehouseLocationId { get; set; }
        public string? Notes { get; set; }
        public List<CreatePurchaseItemDto> Items { get; set; } = new();
    }

    public class CreatePurchaseItemDto
    {
        public int LineNumber { get; set; }
        public Guid ProductId { get; set; }
        public Guid? ProductVariantId { get; set; }
        public string? Description { get; set; }
        public int QuantityReceived { get; set; }
        public decimal UnitCost { get; set; }
        public decimal DiscountPercentage { get; set; } = 0;
        public decimal TaxRate { get; set; } = 0;
        public Guid? GLInventoryAccountId { get; set; }
    }

    public class PurchaseItemDto
    {
        public Guid Id { get; set; }
        public int LineNumber { get; set; }
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public Guid? ProductVariantId { get; set; }
        public string? VariantName { get; set; }
        public string? Description { get; set; }
        public int QuantityOrdered { get; set; }
        public int QuantityReceived { get; set; }
        public decimal UnitCost { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
    }

    // ===== RECEIVE PURCHASE (ACCIÓN CRÍTICA) =====
    public class ReceivePurchaseDto
    {
        public Guid PurchaseReceiptId { get; set; }
        public Guid? DefaultGLInventoryAccountId { get; set; }
        public Guid? DefaultGLAccountsPayableId { get; set; }
        public Guid? DefaultGLTaxAccountId { get; set; }
    }

    // ===== VENDOR INVOICE =====
    public class VendorInvoiceDto
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string InvoiceNumber { get; set; }
        public string? VendorInvoiceReference { get; set; }
        public Guid? PurchaseReceiptId { get; set; }
        public string? PurchaseReceiptNumber { get; set; }
        public Guid VendorId { get; set; }
        public string? VendorName { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountDue => TotalAmount - AmountPaid;
        public InvoiceStatus Status { get; set; }
        public InvoicePaymentStatus PaymentStatus { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateVendorInvoiceDto
    {
        public string? VendorInvoiceReference { get; set; }
        public Guid? PurchaseReceiptId { get; set; }
        public Guid VendorId { get; set; }
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
    }
}
