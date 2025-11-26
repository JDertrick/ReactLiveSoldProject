using System.ComponentModel.DataAnnotations;

namespace ReactLiveSoldProject.ServerBL.Models.Vendors
{
    public class VendorBankAccount
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El ID del proveedor es obligatorio")]
        public Guid VendorId { get; set; }
        public virtual Vendor Vendor { get; set; }

        [Required(ErrorMessage = "El nombre del banco es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre del banco no puede exceder los 100 caracteres")]
        public string BankName { get; set; }

        [Required(ErrorMessage = "El número de cuenta es obligatorio")]
        [MaxLength(50, ErrorMessage = "El número de cuenta no puede exceder los 50 caracteres")]
        public string AccountNumber { get; set; }

        [MaxLength(100, ErrorMessage = "El nombre del titular no puede exceder los 100 caracteres")]
        public string? AccountHolderName { get; set; }

        [MaxLength(50, ErrorMessage = "CLABE/IBAN no puede exceder los 50 caracteres")]
        public string? CLABE_IBAN { get; set; } // CLABE (México), IBAN (Europa)

        [MaxLength(50, ErrorMessage = "El tipo de cuenta no puede exceder los 50 caracteres")]
        public string? AccountType { get; set; } // "Checking", "Savings"

        public bool IsPrimary { get; set; } = false; // Cuenta principal del proveedor

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
