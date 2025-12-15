using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Models.Authentication;
using ReactLiveSoldProject.ServerBL.Models.Contacts;
using ReactLiveSoldProject.ServerBL.Models.Sales;

namespace ReactLiveSoldProject.ServerBL.Models.CustomerWallet
{
    public class Customer
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El ID de la organización es obligatorio")]
        public Guid OrganizationId { get; set; }

        public virtual Organization Organization { get; set; }

        [MaxLength(50, ErrorMessage = "El número de cliente no puede exceder los 50 caracteres")]
        public string? CustomerNo { get; set; } // CUST-2025-0001

        [Required(ErrorMessage = "El ID del contacto es obligatorio")]
        public Guid ContactId { get; set; }

        public virtual Contact Contact { get; set; }

        [Required(ErrorMessage = "El password hash es obligatorio")]
        public string PasswordHash { get; set; }

        public Guid? AssignedSellerId { get; set; }

        public virtual User? AssignedSeller { get; set; }

        [MaxLength(1000, ErrorMessage = "Las notas no pueden exceder los 1000 caracteres")]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Propiedades de navegación
        public virtual Wallet? Wallet { get; set; } // Relación 1-a-1

        public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
    }
}
