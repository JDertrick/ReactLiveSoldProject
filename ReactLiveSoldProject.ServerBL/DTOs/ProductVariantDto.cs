namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class ProductVariantDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string? Sku { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public decimal AverageCost { get; set; }
        public string? Attributes { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
