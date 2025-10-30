namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ProductType { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public List<TagDto> Tags { get; set; } = new();
        public List<ProductVariantDto> Variants { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
