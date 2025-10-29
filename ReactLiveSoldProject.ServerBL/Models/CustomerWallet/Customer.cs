using ReactLiveSoldProject.ServerBL.Models.Authentication;
using ReactLiveSoldProject.ServerBL.Models.Sales;

namespace ReactLiveSoldProject.ServerBL.Models.CustomerWallet
{
    public class Customer
    {
        public Guid Id { get; set; }

        public Guid OrganizationId { get; set; }
        
        public virtual Organization Organization { get; set; }
        
        public string? FirstName { get; set; }
        
        public string? LastName { get; set; }
        
        public string? Email { get; set; }
        
        public string? Phone { get; set; }
        
        public string? PasswordHash { get; set; }
        
        public Guid? AssignedSellerId { get; set; }
        
        public virtual User? AssignedSeller { get; set; }
        
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación
        public virtual Wallet? Wallet { get; set; } // Relación 1-a-1
        
        public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
    }
}
