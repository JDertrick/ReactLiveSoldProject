using ReactLiveSoldProject.ServerBL.Models.Authentication;
using ReactLiveSoldProject.ServerBL.Models.CustomerWallet;

namespace ReactLiveSoldProject.ServerBL.Models.Sales
{
    public class SalesOrder
    {
        public Guid Id { get; set; }
        
        public Guid OrganizationId { get; set; }
        
        public virtual Organization Organization { get; set; }
        
        public Guid CustomerId { get; set; }
        
        public virtual Customer Customer { get; set; }
        
        public Guid? CreatedByUserId { get; set; }
        
        public virtual User? CreatedByUser { get; set; }
        
        public string Status { get; set; } = "draft"; // "draft", "completed", "cancelled"
        
        public decimal TotalAmount { get; set; } = 0.00m;
        
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación
        public virtual ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();
        
        public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
    }
}
