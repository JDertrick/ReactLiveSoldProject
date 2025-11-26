using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Models.Inventory;

namespace ReactLiveSoldProject.ServerBL.Models.Purchases
{
    /// <summary>
    /// PurchaseOrderItem - Items/líneas de una orden de compra
    /// Representa cada producto solicitado en la orden
    /// </summary>
    public class PurchaseOrderItem
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El ID de la orden de compra es obligatorio")]
        public Guid PurchaseOrderId { get; set; }
        public virtual PurchaseOrder PurchaseOrder { get; set; }

        [Required(ErrorMessage = "El número de línea es obligatorio")]
        public int LineNumber { get; set; } // Número de línea en la orden (1, 2, 3...)

        [Required(ErrorMessage = "El ID del producto es obligatorio")]
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; }

        public Guid? ProductVariantId { get; set; }
        public virtual ProductVariant? ProductVariant { get; set; }

        [MaxLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "El costo unitario es obligatorio")]
        public decimal UnitCost { get; set; }

        public decimal DiscountPercentage { get; set; } = 0;

        public decimal DiscountAmount { get; set; } = 0; // Monto del descuento

        public decimal TaxRate { get; set; } = 0; // Porcentaje de impuesto

        public decimal TaxAmount { get; set; } = 0; // Monto del impuesto

        public decimal LineTotal { get; set; } = 0; // Total de la línea (cantidad * costo - descuento + impuesto)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
