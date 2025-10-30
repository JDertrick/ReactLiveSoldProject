using ReactLiveSoldProject.ServerBL.Models.Audit;
using ReactLiveSoldProject.ServerBL.Models.CustomerWallet;
using ReactLiveSoldProject.ServerBL.Models.Sales;

namespace ReactLiveSoldProject.ServerBL.Models.Authentication
{
    public class User
    {
        public Guid Id { get; set; }

        public string? FirstName { get; set; }
        
        public string? LastName { get; set; }
        
        public string Email { get; set; }
        
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
