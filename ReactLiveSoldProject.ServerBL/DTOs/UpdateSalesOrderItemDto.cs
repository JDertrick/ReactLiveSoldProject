using System.ComponentModel.DataAnnotations;

namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class UpdateSalesOrderItemDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
        public int Quantity { get; set; }

        /// <summary>
        /// Precio unitario personalizado (opcional)
        /// </summary>
        public decimal? CustomUnitPrice { get; set; }

        [MaxLength(500)]
        public string? ItemDescription { get; set; }
    }
}
