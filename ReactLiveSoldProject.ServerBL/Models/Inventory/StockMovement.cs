using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.Models.Authentication;
using ReactLiveSoldProject.ServerBL.Models.Sales;

namespace ReactLiveSoldProject.ServerBL.Models.Inventory
{
    /// <summary>
    /// Representa un movimiento de inventario (Ledger)
    /// Cada movimiento registra cambios en el stock de una variante
    /// </summary>
    public class StockMovement
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El ID de la organización es obligatorio")]
        public Guid OrganizationId { get; set; }

        public virtual Organization Organization { get; set; }

        [Required(ErrorMessage = "El ID de la variante del producto es obligatorio")]
        public Guid ProductVariantId { get; set; }

        public virtual ProductVariant ProductVariant { get; set; }

        [Required(ErrorMessage = "El tipo de movimiento es obligatorio")]
        public StockMovementType MovementType { get; set; }

        /// <summary>
        /// Cantidad del movimiento (positiva para entradas, negativa para salidas)
        /// </summary>
        [Required(ErrorMessage = "La cantidad es obligatoria")]
        public int Quantity { get; set; }

        /// <summary>
        /// Stock antes del movimiento
        /// </summary>
        [Required(ErrorMessage = "El stock anterior es obligatorio")]
        public int StockBefore { get; set; }

        /// <summary>
        /// Stock después del movimiento
        /// </summary>
        [Required(ErrorMessage = "El stock posterior es obligatorio")]
        public int StockAfter { get; set; }

        /// <summary>
        /// ID de la orden de venta relacionada (si aplica)
        /// </summary>
        public Guid? RelatedSalesOrderId { get; set; }

        public virtual SalesOrder? RelatedSalesOrder { get; set; }

        /// <summary>
        /// Usuario que realizó el movimiento
        /// </summary>
        [Required(ErrorMessage = "El ID del usuario es obligatorio")]
        public Guid CreatedByUserId { get; set; }

        public virtual User CreatedByUser { get; set; }

        /// <summary>
        /// Notas o descripción del movimiento
        /// </summary>
        [MaxLength(500, ErrorMessage = "Las notas no pueden exceder los 500 caracteres")]
        public string? Notes { get; set; }

        /// <summary>
        /// Referencia externa (número de factura, orden de compra, etc.)
        /// </summary>
        [MaxLength(100, ErrorMessage = "La referencia no puede exceder los 100 caracteres")]
        public string? Reference { get; set; }

        /// <summary>
        /// Costo unitario del producto en el momento del movimiento (útil para movimientos de compra)
        /// </summary>
        public decimal? UnitCost { get; set; }

        /// <summary>
        /// Ubicación de origen para transferencias (null para otros tipos de movimientos)
        /// </summary>
        public Guid? SourceLocationId { get; set; }

        public virtual Location? SourceLocation { get; set; }

        /// <summary>
        /// Ubicación de destino (obligatoria para todos los movimientos excepto ventas)
        /// Para transferencias: ubicación destino
        /// Para otros movimientos: ubicación donde ocurre el movimiento
        /// </summary>
        public Guid? DestinationLocationId { get; set; }

        public virtual Location? DestinationLocation { get; set; }

        /// <summary>
        /// Indica si el movimiento ha sido posteado y afecta el inventario
        /// Los movimientos no posteados son borradores que no afectan el stock
        /// </summary>
        public bool IsPosted { get; set; } = false;

        /// <summary>
        /// Fecha y hora en que el movimiento fue posteado
        /// </summary>
        public DateTime? PostedAt { get; set; }

        /// <summary>
        /// Usuario que posteó el movimiento
        /// </summary>
        public Guid? PostedByUserId { get; set; }

        public virtual User? PostedByUser { get; set; }

        public bool IsRejected { get; set; } = false;
        public DateTime? RejectedAt { get; set; }
        public Guid? RejectedByUserId { get; set; }
        public virtual User? RejectedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
