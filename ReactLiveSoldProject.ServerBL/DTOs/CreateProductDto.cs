using System.ComponentModel.DataAnnotations;

namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class CreateProductDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public string ProductType { get; set; } = "Simple";

        public bool IsPublished { get; set; } = true;

        public decimal BasePrice { get; set; } = 0.00m;

        public Guid? CategoryId { get; set; }
        public Guid? LocationId { get; set; }

        /// <summary>
        /// Lista de IDs de tags a asociar con el producto
        /// </summary>
        public List<Guid> TagIds { get; set; } = new();

        /// <summary>
        /// Lista de variantes del producto
        /// </summary>
        public List<CreateProductVariantDto> Variants { get; set; } = new();
    }
}
