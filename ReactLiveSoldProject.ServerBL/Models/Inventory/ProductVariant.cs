using ReactLiveSoldProject.ServerBL.Models.Authentication;
using ReactLiveSoldProject.ServerBL.Models.Sales;

namespace ReactLiveSoldProject.ServerBL.Models.Inventory
{
    public class ProductVariant
    {
        public Guid Id { get; set; }
        
        public Guid OrganizationId { get; set; }
        
        public virtual Organization Organization { get; set; }
        
        public Guid ProductId { get; set; }
        
        public virtual Product Product { get; set; }
        
        public string? Sku { get; set; }
        
        public decimal Price { get; set; } = 0.00m;
        
        public int StockQuantity { get; set; } = 0;
        
        public string? Attributes { get; set; } // Mapeado a JSONB en DbContext
        
        public string? ImageUrl { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación
        public virtual ICollection<SalesOrderItem> SalesOrderItems { get; set; } = new List<SalesOrderItem>();
    }
}
