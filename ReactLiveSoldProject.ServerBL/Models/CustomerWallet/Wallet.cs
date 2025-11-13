using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Models.Authentication;

namespace ReactLiveSoldProject.ServerBL.Models.CustomerWallet
{
    public class Wallet
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El ID de la organización es obligatorio")]
        public Guid OrganizationId { get; set; }

        public virtual Organization Organization { get; set; }

        [Required(ErrorMessage = "El ID del cliente es obligatorio")]
        public Guid CustomerId { get; set; }

        public virtual Customer Customer { get; set; }

        [Required(ErrorMessage = "El balance es obligatorio")]
        [Range(0, double.MaxValue, ErrorMessage = "El balance no puede ser negativo")]
        public decimal Balance { get; set; } = 0.00m;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación
        public virtual ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
    }
}
