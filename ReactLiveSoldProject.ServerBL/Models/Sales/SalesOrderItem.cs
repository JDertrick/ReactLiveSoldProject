using ReactLiveSoldProject.ServerBL.Models.Authentication;
using ReactLiveSoldProject.ServerBL.Models.Inventory;

namespace ReactLiveSoldProject.ServerBL.Models.Sales
{
    public class SalesOrderItem
    {
        public Guid Id { get; set; }
        
        public Guid OrganizationId { get; set; }
        
        public virtual Organization Organization { get; set; }
        
        public Guid SalesOrderId { get; set; }
        
        public virtual SalesOrder SalesOrder { get; set; }
        
        public Guid ProductVariantId { get; set; }
        
        public virtual ProductVariant ProductVariant { get; set; }
        
        public int Quantity { get; set; } = 1;
        
        public decimal OriginalPrice { get; set; }
        
        public decimal UnitPrice { get; set; } // El precio editable de la venta LIVE
        
        public string? ItemDescription { get; set; }
    }
}
