using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Models.Authentication;
using ReactLiveSoldProject.ServerBL.Models.Contacts;

namespace ReactLiveSoldProject.ServerBL.Models.Vendors
{
    public class Vendor
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El ID de la organización es obligatorio")]
        public Guid OrganizationId { get; set; }

        public virtual Organization Organization { get; set; }

        [Required(ErrorMessage = "El ID del contacto es obligatorio")]
        public Guid ContactId { get; set; }

        public virtual Contact Contact { get; set; }

        public Guid? AssignedBuyerId { get; set; }

        public virtual User? AssignedBuyer { get; set; }

        [MaxLength(50, ErrorMessage = "El código del proveedor no puede exceder los 50 caracteres")]
        public string? VendorCode { get; set; }

        [MaxLength(1000, ErrorMessage = "Las notas no pueden exceder los 1000 caracteres")]
        public string? Notes { get; set; }

        [MaxLength(50, ErrorMessage = "Los términos de pago no pueden exceder los 50 caracteres")]
        public string? PaymentTerms { get; set; }

        [MaxLength(50, ErrorMessage = "El RFC/NIT/Tax ID no puede exceder los 50 caracteres")]
        public string? TaxId { get; set; } // RFC (México), NIT (Colombia), etc.

        [MaxLength(3, ErrorMessage = "La moneda debe ser un código de 3 caracteres")]
        public string Currency { get; set; } = "MXN"; // USD, MXN, COP, etc.

        public Guid? PaymentTermsId { get; set; } // FK a PaymentTerms

        public decimal CreditLimit { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
