using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Models.Authentication;
using ReactLiveSoldProject.ServerBL.Models.Sales;

namespace ReactLiveSoldProject.ServerBL.Models.Inventory
{
    public class ProductVariant
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El ID de la organización es obligatorio")]
        public Guid OrganizationId { get; set; }

        public virtual Organization Organization { get; set; }

        [Required(ErrorMessage = "El ID del producto es obligatorio")]
        public Guid ProductId { get; set; }

        public virtual Product Product { get; set; }

        [MaxLength(100, ErrorMessage = "El SKU no puede exceder los 100 caracteres")]
        public string? Sku { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser mayor o igual a 0")]
        public decimal Price { get; set; } = 0.00m;

        [Required(ErrorMessage = "La cantidad en stock es obligatoria")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int StockQuantity { get; set; } = 0;

        public string? Attributes { get; set; } // Mapeado a JSONB en DbContext

        [Url(ErrorMessage = "La URL de la imagen debe ser válida")]
        [MaxLength(500, ErrorMessage = "La URL de la imagen no puede exceder los 500 caracteres")]
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación
        public virtual ICollection<SalesOrderItem> SalesOrderItems { get; set; } = new List<SalesOrderItem>();
    }
}
