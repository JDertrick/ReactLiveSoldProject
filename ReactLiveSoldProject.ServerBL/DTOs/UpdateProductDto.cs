using System.ComponentModel.DataAnnotations;

namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class UpdateProductDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public string ProductType { get; set; } = "Simple";

        public decimal BasePrice { get; set; } = 0.00m;

        public string? ImageUrl { get; set; } = null;

        public bool IsPublished { get; set; } = true;

        /// <summary>
        /// Lista de IDs de tags a asociar con el producto
        /// </summary>
        public List<Guid> TagIds { get; set; } = new();

        /// <summary>
        /// Variantes del producto - si se incluye, reemplazará todas las variantes existentes
        /// </summary>
        public List<CreateProductVariantDto>? Variants { get; set; }
    }
}
