using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Models.Authentication;
using ReactLiveSoldProject.ServerBL.Models.Accounting;

namespace ReactLiveSoldProject.ServerBL.Models.Banking
{
    public class CompanyBankAccount
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El ID de la organización es obligatorio")]
        public Guid OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }

        [Required(ErrorMessage = "El nombre del banco es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre del banco no puede exceder los 100 caracteres")]
        public string BankName { get; set; }

        [Required(ErrorMessage = "El número de cuenta es obligatorio")]
        [MaxLength(50, ErrorMessage = "El número de cuenta no puede exceder los 50 caracteres")]
        public string AccountNumber { get; set; }

        [MaxLength(3, ErrorMessage = "La moneda debe ser un código de 3 caracteres")]
        public string Currency { get; set; } = "MXN"; // USD, MXN, COP, etc.

        public decimal CurrentBalance { get; set; } = 0; // Saldo actual de la cuenta

        // Vínculo al plan de cuentas contable
        [Required(ErrorMessage = "La cuenta contable vinculada es obligatoria")]
        public Guid GLAccountId { get; set; }
        public virtual ChartOfAccount GLAccount { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
