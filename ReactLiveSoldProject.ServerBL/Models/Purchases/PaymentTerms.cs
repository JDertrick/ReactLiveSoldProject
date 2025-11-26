using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Models.Authentication;

namespace ReactLiveSoldProject.ServerBL.Models.Purchases
{
    public class PaymentTerms
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El ID de la organización es obligatorio")]
        public Guid OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [MaxLength(100, ErrorMessage = "La descripción no puede exceder los 100 caracteres")]
        public string Description { get; set; } // "Net 30", "Inmediato", "2/10 Net 30"

        public int DueDays { get; set; } = 0; // Días para pagar después de la factura

        public decimal DiscountPercentage { get; set; } = 0; // % descuento por pronto pago

        public int DiscountDays { get; set; } = 0; // Días para aplicar el descuento

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
