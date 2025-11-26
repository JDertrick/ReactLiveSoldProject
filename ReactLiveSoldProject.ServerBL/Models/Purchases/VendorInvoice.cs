using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.Models.Authentication;
using ReactLiveSoldProject.ServerBL.Models.Vendors;

namespace ReactLiveSoldProject.ServerBL.Models.Purchases
{
    public class VendorInvoice
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El ID de la organización es obligatorio")]
        public Guid OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }

        [Required(ErrorMessage = "El número de factura es obligatorio")]
        [MaxLength(50, ErrorMessage = "El número de factura no puede exceder los 50 caracteres")]
        public string InvoiceNumber { get; set; } // INV-00001 (nuestro número interno)

        [MaxLength(100, ErrorMessage = "La referencia del proveedor no puede exceder los 100 caracteres")]
        public string? VendorInvoiceReference { get; set; } // Número de factura del proveedor

        public Guid? PurchaseReceiptId { get; set; }
        public virtual PurchaseReceipt? PurchaseReceipt { get; set; }

        [Required(ErrorMessage = "El ID del proveedor es obligatorio")]
        public Guid VendorId { get; set; }
        public virtual Vendor Vendor { get; set; }

        [Required(ErrorMessage = "La fecha de factura es obligatoria")]
        public DateTime InvoiceDate { get; set; }

        [Required(ErrorMessage = "La fecha de vencimiento es obligatoria")]
        public DateTime DueDate { get; set; }

        public decimal Subtotal { get; set; } = 0;

        public decimal TaxAmount { get; set; } = 0;

        public decimal TotalAmount { get; set; } = 0;

        public decimal AmountPaid { get; set; } = 0; // Monto ya pagado

        public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;

        public InvoicePaymentStatus PaymentStatus { get; set; } = InvoicePaymentStatus.Unpaid;

        [MaxLength(2000, ErrorMessage = "Las notas no pueden exceder los 2000 caracteres")]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
