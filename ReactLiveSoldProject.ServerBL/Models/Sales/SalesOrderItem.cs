using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Models.Authentication;
using ReactLiveSoldProject.ServerBL.Models.Inventory;
using ReactLiveSoldProject.ServerBL.Models.Taxes;

namespace ReactLiveSoldProject.ServerBL.Models.Sales
{
    public class SalesOrderItem
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El ID de la organización es obligatorio")]
        public Guid OrganizationId { get; set; }

        public virtual Organization Organization { get; set; }

        [Required(ErrorMessage = "El ID de la orden de venta es obligatorio")]
        public Guid SalesOrderId { get; set; }

        public virtual SalesOrder SalesOrder { get; set; }

        [Required(ErrorMessage = "El ID de la variante del producto es obligatorio")]
        public Guid ProductVariantId { get; set; }

        public virtual ProductVariant ProductVariant { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Quantity { get; set; } = 1;

        [Required(ErrorMessage = "El precio original es obligatorio")]
        [Range(0, double.MaxValue, ErrorMessage = "El precio original debe ser mayor o igual a 0")]
        public decimal OriginalPrice { get; set; }

        [Required(ErrorMessage = "El precio unitario es obligatorio")]
        [Range(0, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor o igual a 0")]
        public decimal UnitPrice { get; set; } // El precio editable de la venta LIVE

        /// <summary>
        /// Costo unitario del producto al momento de la venta (costo promedio ponderado)
        /// Utilizado para calcular la ganancia/margen
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "El costo unitario debe ser mayor o igual a 0")]
        public decimal UnitCost { get; set; } = 0.00m;

        [MaxLength(500, ErrorMessage = "La descripción del item no puede exceder los 500 caracteres")]
        public string? ItemDescription { get; set; }

        // ==================== CAMPOS DE IMPUESTOS ====================

        /// <summary>
        /// ID de la tasa de impuesto aplicada a este item (nullable si no aplica impuesto)
        /// </summary>
        public Guid? TaxRateId { get; set; }

        /// <summary>
        /// Tasa de impuesto aplicada (ej: 0.19 para 19%)
        /// Se guarda para histórico, aunque la tasa cambie después
        /// </summary>
        [Range(0, 1, ErrorMessage = "La tasa de impuesto debe estar entre 0 y 1")]
        public decimal TaxRate { get; set; } = 0.00m;

        /// <summary>
        /// Monto de impuesto calculado para este item
        /// TaxAmount = (UnitPrice * Quantity * TaxRate) dependiendo del modo de aplicación
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "El monto de impuesto debe ser mayor o igual a 0")]
        public decimal TaxAmount { get; set; } = 0.00m;

        /// <summary>
        /// Subtotal sin impuestos (UnitPrice * Quantity)
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "El subtotal debe ser mayor o igual a 0")]
        public decimal Subtotal { get; set; } = 0.00m;

        /// <summary>
        /// Total del item incluyendo impuestos (Subtotal + TaxAmount)
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "El total debe ser mayor o igual a 0")]
        public decimal Total { get; set; } = 0.00m;

        // Relaciones
        public virtual TaxRate? TaxRateEntity { get; set; }
    }
}
