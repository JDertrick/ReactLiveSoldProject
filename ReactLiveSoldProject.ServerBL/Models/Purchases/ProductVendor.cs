using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Models.Inventory;
using ReactLiveSoldProject.ServerBL.Models.Vendors;

namespace ReactLiveSoldProject.ServerBL.Models.Purchases
{
    public class ProductVendor
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El ID del producto es obligatorio")]
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Required(ErrorMessage = "El ID del proveedor es obligatorio")]
        public Guid VendorId { get; set; }
        public virtual Vendor Vendor { get; set; }

        [MaxLength(100, ErrorMessage = "El SKU del proveedor no puede exceder los 100 caracteres")]
        public string? VendorSKU { get; set; } // SKU/código del proveedor para este producto

        [Required(ErrorMessage = "El precio de costo es obligatorio")]
        public decimal CostPrice { get; set; } // Precio de compra al proveedor

        public int LeadTimeDays { get; set; } = 0; // Días de entrega del proveedor

        public int MinimumOrderQuantity { get; set; } = 1; // Cantidad mínima de pedido

        public bool IsPreferred { get; set; } = false; // Proveedor preferido para este producto

        public DateTime ValidFrom { get; set; } = DateTime.UtcNow; // Vigencia del precio desde

        public DateTime? ValidTo { get; set; } // Vigencia del precio hasta (null = indefinido)

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
