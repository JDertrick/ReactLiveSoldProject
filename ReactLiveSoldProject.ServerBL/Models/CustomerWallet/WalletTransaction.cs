using ReactLiveSoldProject.ServerBL.Models.Authentication;
using ReactLiveSoldProject.ServerBL.Models.Sales;

namespace ReactLiveSoldProject.ServerBL.Models.CustomerWallet
{
    public class WalletTransaction
    {
        public Guid Id { get; set; }
        
        public Guid OrganizationId { get; set; }
        
        public virtual Organization Organization { get; set; }
        
        public Guid WalletId { get; set; }
        
        public virtual Wallet Wallet { get; set; }
        
        public string Type { get; set; } // "deposit" o "withdrawal"
        
        public decimal Amount { get; set; }
        
        public Guid? RelatedSalesOrderId { get; set; }
        
        public virtual SalesOrder? RelatedSalesOrder { get; set; }
        
        public Guid? AuthorizedByUserId { get; set; }
        
        public virtual User? AuthorizedByUser { get; set; }
        
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
