using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Models.Inventory;
using ReactLiveSoldProject.ServerBL.Models.Accounting;

namespace ReactLiveSoldProject.ServerBL.Models.Purchases
{
    public class PurchaseItem
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El número de línea es obligatorio")]
        public int LineNumber { get; set; } // Número de línea en la orden

        [Required(ErrorMessage = "El ID de recepción de compra es obligatorio")]
        public Guid PurchaseReceiptId { get; set; }
        public virtual PurchaseReceipt PurchaseReceipt { get; set; }

        [Required(ErrorMessage = "El ID del producto es obligatorio")]
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; }

        public Guid? ProductVariantId { get; set; }
        public virtual ProductVariant? ProductVariant { get; set; }

        [MaxLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        public string? Description { get; set; }

        public int QuantityOrdered { get; set; } = 0; // Cantidad ordenada (si viene de PO)

        [Required(ErrorMessage = "La cantidad recibida es obligatoria")]
        public int QuantityReceived { get; set; } // Cantidad realmente recibida

        [Required(ErrorMessage = "El costo unitario es obligatorio")]
        public decimal UnitCost { get; set; }

        public decimal DiscountPercentage { get; set; } = 0;

        public decimal TaxRate { get; set; } = 0; // Porcentaje de impuesto

        public decimal TaxAmount { get; set; } = 0; // Monto del impuesto

        public decimal LineTotal { get; set; } = 0; // Total de la línea (incluyendo impuesto)

        // Cuenta contable de inventario (normalmente será la misma para todos los productos)
        public Guid? GLInventoryAccountId { get; set; }
        public virtual ChartOfAccount? GLInventoryAccount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
