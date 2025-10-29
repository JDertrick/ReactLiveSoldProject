using ReactLiveSoldProject.ServerBL.Models.Authentication;

namespace ReactLiveSoldProject.ServerBL.Models.CustomerWallet
{
    public class Wallet
    {
        public Guid Id { get; set; }

        public Guid OrganizationId { get; set; }
        
        public virtual Organization Organization { get; set; }
        
        public Guid CustomerId { get; set; }
        
        public virtual Customer Customer { get; set; }
        
        public decimal Balance { get; set; } = 0.00m;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación
        public virtual ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
    }
}
