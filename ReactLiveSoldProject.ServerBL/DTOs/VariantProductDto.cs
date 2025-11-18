using ReactLiveSoldProject.ServerBL.Base;

namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class VariantProductDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string? Sku { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public decimal AverageCost { get; set; }
        public string? Attributes { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsPublished { get; set; }
        public string? ProductType { get; set; }
        public ProductDto? Product { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
