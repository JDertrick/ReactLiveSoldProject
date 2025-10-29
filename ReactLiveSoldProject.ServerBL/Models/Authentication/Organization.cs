using ReactLiveSoldProject.ServerBL.Models.Audit;
using ReactLiveSoldProject.ServerBL.Models.CustomerWallet;
using ReactLiveSoldProject.ServerBL.Models.Inventory;
using ReactLiveSoldProject.ServerBL.Models.Sales;

namespace ReactLiveSoldProject.ServerBL.Models.Authentication
{
    public class Organization
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        
        public string? LogoUrl { get; set; }
        
        public string PrimaryContactEmail { get; set; }
        
        public string PlanType { get; set; } = "standard";
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación
        public virtual ICollection<OrganizationMember> Members { get; set; } = new List<OrganizationMember>();

        public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
        
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        
        public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
        
        public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
        
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
