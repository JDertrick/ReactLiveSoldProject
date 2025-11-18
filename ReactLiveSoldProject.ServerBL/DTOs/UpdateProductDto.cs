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

        public bool IsPublished { get; set; } = true;

        public Guid? CategoryId { get; set; }
    }
}
