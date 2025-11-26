using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.Models.Authentication;
using ReactLiveSoldProject.ServerBL.Models.Vendors;
using ReactLiveSoldProject.ServerBL.Models.Banking;
using ReactLiveSoldProject.ServerBL.Models.Accounting;

namespace ReactLiveSoldProject.ServerBL.Models.Payments
{
    public class Payment
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El ID de la organización es obligatorio")]
        public Guid OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }

        [Required(ErrorMessage = "El número de pago es obligatorio")]
        [MaxLength(50, ErrorMessage = "El número de pago no puede exceder los 50 caracteres")]
        public string PaymentNumber { get; set; } // PAY-00001

        [Required(ErrorMessage = "La fecha de pago es obligatoria")]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "El ID del proveedor es obligatorio")]
        public Guid VendorId { get; set; }
        public virtual Vendor Vendor { get; set; }

        [Required(ErrorMessage = "El método de pago es obligatorio")]
        public PaymentMethod PaymentMethod { get; set; }

        [Required(ErrorMessage = "La cuenta bancaria es obligatoria")]
        public Guid CompanyBankAccountId { get; set; }
        public virtual CompanyBankAccount CompanyBankAccount { get; set; }

        public Guid? VendorBankAccountId { get; set; }
        public virtual VendorBankAccount? VendorBankAccount { get; set; }

        [Required(ErrorMessage = "El monto pagado es obligatorio")]
        public decimal AmountPaid { get; set; }

        [MaxLength(3, ErrorMessage = "La moneda debe ser un código de 3 caracteres")]
        public string Currency { get; set; } = "MXN";

        public decimal ExchangeRate { get; set; } = 1.0m; // Tipo de cambio si es moneda extranjera

        [MaxLength(100, ErrorMessage = "El número de referencia no puede exceder los 100 caracteres")]
        public string? ReferenceNumber { get; set; } // Número de transferencia/cheque

        [MaxLength(2000, ErrorMessage = "Las notas no pueden exceder los 2000 caracteres")]
        public string? Notes { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Posted;

        // Asiento contable generado automáticamente
        public Guid? PaymentJournalEntryId { get; set; }
        public virtual JournalEntry? PaymentJournalEntry { get; set; }

        [Required(ErrorMessage = "El ID del creador es obligatorio")]
        public Guid CreatedBy { get; set; }
        public virtual User CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property para las aplicaciones del pago a facturas
        public virtual ICollection<PaymentApplication> Applications { get; set; } = new List<PaymentApplication>();
    }
}
