using System.ComponentModel.DataAnnotations;

namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class CreateProductVariantDto
    {
        [MaxLength(100)]
        public string? Sku { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; } = 0;

        /// <summary>
        /// JSON string con atributos de la variante (ej: {"color": "rojo", "talla": "M"})
        /// </summary>
        public string? Attributes { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }
    }
}
