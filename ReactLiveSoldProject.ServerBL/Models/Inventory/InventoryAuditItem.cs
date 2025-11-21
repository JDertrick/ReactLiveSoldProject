using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Models.Authentication;

namespace ReactLiveSoldProject.ServerBL.Models.Inventory
{
    /// <summary>
    /// Representa un item dentro de una auditoría de inventario
    /// Contiene el snapshot del stock teórico y el conteo físico
    /// </summary>
    public class InventoryAuditItem
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El ID de la auditoría es obligatorio")]
        public Guid InventoryAuditId { get; set; }

        public virtual InventoryAudit InventoryAudit { get; set; } = null!;

        [Required(ErrorMessage = "El ID de la variante es obligatorio")]
        public Guid ProductVariantId { get; set; }

        public virtual ProductVariant ProductVariant { get; set; } = null!;

        /// <summary>
        /// Stock teórico al momento de crear la auditoría (Snapshot)
        /// </summary>
        [Required]
        public int TheoreticalStock { get; set; }

        /// <summary>
        /// Stock contado físicamente (null hasta que se realice el conteo)
        /// </summary>
        public int? CountedStock { get; set; }

        /// <summary>
        /// Diferencia entre conteo y stock teórico (CountedStock - TheoreticalStock)
        /// Positivo = sobrante, Negativo = faltante
        /// </summary>
        public int? Variance { get; set; }

        /// <summary>
        /// Valor monetario de la varianza (Variance * AverageCost)
        /// </summary>
        public decimal? VarianceValue { get; set; }

        /// <summary>
        /// Costo promedio al momento del snapshot (para calcular valor de varianza)
        /// </summary>
        public decimal SnapshotAverageCost { get; set; }

        /// <summary>
        /// Usuario que realizó el conteo
        /// </summary>
        public Guid? CountedByUserId { get; set; }

        public virtual User? CountedByUser { get; set; }

        /// <summary>
        /// Fecha y hora del conteo
        /// </summary>
        public DateTime? CountedAt { get; set; }

        /// <summary>
        /// ID del movimiento de ajuste generado (si hubo varianza)
        /// </summary>
        public Guid? AdjustmentMovementId { get; set; }

        public virtual StockMovement? AdjustmentMovement { get; set; }

        [MaxLength(500, ErrorMessage = "Las notas no pueden exceder los 500 caracteres")]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
