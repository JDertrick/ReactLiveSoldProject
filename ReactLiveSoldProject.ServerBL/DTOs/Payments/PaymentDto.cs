using ReactLiveSoldProject.ServerBL.Base;

namespace ReactLiveSoldProject.ServerBL.DTOs.Payments
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string PaymentNumber { get; set; }
        public DateTime PaymentDate { get; set; }
        public Guid VendorId { get; set; }
        public string? VendorName { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public Guid CompanyBankAccountId { get; set; }
        public string? CompanyBankAccountName { get; set; }
        public Guid? VendorBankAccountId { get; set; }
        public string? VendorBankAccountName { get; set; }
        public decimal AmountPaid { get; set; }
        public string Currency { get; set; }
        public decimal ExchangeRate { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public PaymentStatus Status { get; set; }
        public Guid? PaymentJournalEntryId { get; set; }
        public Guid CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<PaymentApplicationDto>? Applications { get; set; }
    }

    public class CreatePaymentDto
    {
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public Guid VendorId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public Guid CompanyBankAccountId { get; set; }
        public Guid? VendorBankAccountId { get; set; }
        public decimal AmountPaid { get; set; }
        public string Currency { get; set; } = "MXN";
        public decimal ExchangeRate { get; set; } = 1.0m;
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }

        // Lista de facturas a pagar con este pago
        public List<PaymentInvoiceApplicationDto> InvoiceApplications { get; set; } = new();

        // IDs de cuentas contables para el asiento automático
        public Guid? GLAccountsPayableId { get; set; }
    }

    public class PaymentInvoiceApplicationDto
    {
        public Guid VendorInvoiceId { get; set; }
        public decimal AmountApplied { get; set; }
        public decimal DiscountTaken { get; set; } = 0;
    }

    public class PaymentApplicationDto
    {
        public Guid Id { get; set; }
        public Guid PaymentId { get; set; }
        public Guid VendorInvoiceId { get; set; }
        public string? VendorInvoiceNumber { get; set; }
        public decimal AmountApplied { get; set; }
        public decimal DiscountTaken { get; set; }
        public DateTime ApplicationDate { get; set; }
    }

    public class VoidPaymentDto
    {
        public string? Reason { get; set; }
    }
}
