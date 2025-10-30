using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Models.Authentication;
using ReactLiveSoldProject.ServerBL.Models.Sales;

namespace ReactLiveSoldProject.ServerBL.Models.CustomerWallet
{
    public class Customer
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El ID de la organización es obligatorio")]
        public Guid OrganizationId { get; set; }

        public virtual Organization Organization { get; set; }

        [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string? FirstName { get; set; }

        [MaxLength(100, ErrorMessage = "El apellido no puede exceder los 100 caracteres")]
        public string? LastName { get; set; }

        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [MaxLength(255, ErrorMessage = "El email no puede exceder los 255 caracteres")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        [MaxLength(20, ErrorMessage = "El teléfono no puede exceder los 20 caracteres")]
        public string? Phone { get; set; }

        public string? PasswordHash { get; set; }

        public Guid? AssignedSellerId { get; set; }

        public virtual User? AssignedSeller { get; set; }

        [MaxLength(1000, ErrorMessage = "Las notas no pueden exceder los 1000 caracteres")]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación
        public virtual Wallet? Wallet { get; set; } // Relación 1-a-1
        
        public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
    }
}
