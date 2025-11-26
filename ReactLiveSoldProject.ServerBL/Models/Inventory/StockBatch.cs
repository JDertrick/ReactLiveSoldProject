using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Models.Purchases;

namespace ReactLiveSoldProject.ServerBL.Models.Inventory
{
    /// <summary>
    /// Lotes de inventario para control FIFO (First In, First Out)
    /// Cada recepción de compra crea un lote que se va consumiendo en orden
    /// </summary>
    public class StockBatch
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El ID del producto es obligatorio")]
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; }

        public Guid? ProductVariantId { get; set; }
        public virtual ProductVariant? ProductVariant { get; set; }

        public Guid? PurchaseReceiptId { get; set; } // Referencia a la recepción que creó este lote
        public virtual PurchaseReceipt? PurchaseReceipt { get; set; }

        [Required(ErrorMessage = "La cantidad restante es obligatoria")]
        public int QuantityRemaining { get; set; } // Cantidad disponible en este lote

        [Required(ErrorMessage = "El costo unitario es obligatorio")]
        public decimal UnitCost { get; set; } // Costo unitario del lote (para FIFO costing)

        [Required(ErrorMessage = "La fecha de recepción es obligatoria")]
        public DateTime ReceiptDate { get; set; } // Fecha de recepción (para orden FIFO)

        public Guid? LocationId { get; set; }
        public virtual Location? Location { get; set; }

        public bool IsActive { get; set; } = true; // False cuando QuantityRemaining = 0

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
