using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Models.Audit;
using ReactLiveSoldProject.ServerBL.Models.CustomerWallet;
using ReactLiveSoldProject.ServerBL.Models.Sales;

namespace ReactLiveSoldProject.ServerBL.Models.Authentication
{
    public class User
    {
        public Guid Id { get; set; }

        [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string? FirstName { get; set; }

        [MaxLength(100, ErrorMessage = "El apellido no puede exceder los 100 caracteres")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [MaxLength(255, ErrorMessage = "El email no puede exceder los 255 caracteres")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El password hash es obligatorio")]
        public string PasswordHash { get; set; }

        public bool IsSuperAdmin { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación
        public virtual ICollection<OrganizationMember> OrganizationLinks { get; set; } = new List<OrganizationMember>();
        
        public virtual ICollection<Customer> AssignedCustomers { get; set; } = new List<Customer>();
        
        public virtual ICollection<WalletTransaction> AuthorizedTransactions { get; set; } = new List<WalletTransaction>();
        
        public virtual ICollection<SalesOrder> CreatedSalesOrders { get; set; } = new List<SalesOrder>();
        
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
