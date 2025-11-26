using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.Models.Authentication;
using ReactLiveSoldProject.ServerBL.Models.Vendors;
using ReactLiveSoldProject.ServerBL.Models.Inventory;
using ReactLiveSoldProject.ServerBL.Models.Accounting;

namespace ReactLiveSoldProject.ServerBL.Models.Purchases
{
    public class PurchaseReceipt
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El ID de la organización es obligatorio")]
        public Guid OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }

        [Required(ErrorMessage = "El número de recepción es obligatorio")]
        [MaxLength(50, ErrorMessage = "El número de recepción no puede exceder los 50 caracteres")]
        public string ReceiptNumber { get; set; } // RCV-00001

        public Guid? PurchaseOrderId { get; set; } // Puede ser null (compra directa sin PO)
        public virtual PurchaseOrder? PurchaseOrder { get; set; }

        [Required(ErrorMessage = "El ID del proveedor es obligatorio")]
        public Guid VendorId { get; set; }
        public virtual Vendor Vendor { get; set; }

        [Required(ErrorMessage = "La fecha de recepción es obligatoria")]
        public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "El estado es obligatorio")]
        public PurchaseReceiptStatus Status { get; set; } = PurchaseReceiptStatus.Pending;

        public Guid? WarehouseLocationId { get; set; }
        public virtual Location? WarehouseLocation { get; set; }

        [Required(ErrorMessage = "El ID del usuario que recibe es obligatorio")]
        public Guid ReceivedBy { get; set; }
        public virtual User ReceivedByUser { get; set; }

        [MaxLength(2000, ErrorMessage = "Las notas no pueden exceder los 2000 caracteres")]
        public string? Notes { get; set; }

        // Vínculo al asiento contable generado automáticamente
        public Guid? ReceivingJournalEntryId { get; set; }
        public virtual JournalEntry? ReceivingJournalEntry { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navegación
        public virtual ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
        public virtual ICollection<VendorInvoice> VendorInvoices { get; set; } = new List<VendorInvoice>();
    }
}
