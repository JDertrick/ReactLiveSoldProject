using System.ComponentModel.DataAnnotations;

namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class CreateSalesOrderItemDto
    {
        [Required]
        public Guid ProductVariantId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
        public int Quantity { get; set; }

        /// <summary>
        /// Precio unitario personalizado (opcional, si no se envía se usa el precio del producto)
        /// </summary>
        public decimal? CustomUnitPrice { get; set; }

        [MaxLength(500)]
        public string? ItemDescription { get; set; }
    }
}
