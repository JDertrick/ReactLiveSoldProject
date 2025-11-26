using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.Models.Authentication;
using ReactLiveSoldProject.ServerBL.Models.Vendors;

namespace ReactLiveSoldProject.ServerBL.Models.Purchases
{
    public class PurchaseOrder
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El ID de la organización es obligatorio")]
        public Guid OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }

        [Required(ErrorMessage = "El número de orden es obligatorio")]
        [MaxLength(50, ErrorMessage = "El número de orden no puede exceder los 50 caracteres")]
        public string PONumber { get; set; } // PO-00001

        [Required(ErrorMessage = "El ID del proveedor es obligatorio")]
        public Guid VendorId { get; set; }
        public virtual Vendor Vendor { get; set; }

        [Required(ErrorMessage = "La fecha de orden es obligatoria")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public DateTime? ExpectedDeliveryDate { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

        public decimal Subtotal { get; set; } = 0;

        public decimal TaxAmount { get; set; } = 0;

        public decimal TotalAmount { get; set; } = 0;

        [MaxLength(3, ErrorMessage = "La moneda debe ser un código de 3 caracteres")]
        public string Currency { get; set; } = "MXN";

        public decimal ExchangeRate { get; set; } = 1.0m; // Tipo de cambio si es moneda extranjera

        public Guid? PaymentTermsId { get; set; }
        public virtual PaymentTerms? PaymentTerms { get; set; }

        [MaxLength(2000, ErrorMessage = "Las notas no pueden exceder los 2000 caracteres")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "El ID del creador es obligatorio")]
        public Guid CreatedBy { get; set; }
        public virtual User CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navegación
        public virtual ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
        public virtual ICollection<PurchaseReceipt> PurchaseReceipts { get; set; } = new List<PurchaseReceipt>();
    }
}
