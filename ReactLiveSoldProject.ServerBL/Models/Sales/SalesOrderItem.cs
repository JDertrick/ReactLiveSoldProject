using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Models.Authentication;
using ReactLiveSoldProject.ServerBL.Models.Inventory;

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
    }
}
