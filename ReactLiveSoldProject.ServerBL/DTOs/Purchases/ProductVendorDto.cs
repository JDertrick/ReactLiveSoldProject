namespace ReactLiveSoldProject.ServerBL.DTOs.Purchases
{
    public class ProductVendorDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public Guid VendorId { get; set; }
        public string? VendorName { get; set; }
        public string? VendorSKU { get; set; }
        public decimal CostPrice { get; set; }
        public int LeadTimeDays { get; set; }
        public int MinimumOrderQuantity { get; set; }
        public bool IsPreferred { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateProductVendorDto
    {
        public Guid ProductId { get; set; }
        public Guid VendorId { get; set; }
        public string? VendorSKU { get; set; }
        public decimal CostPrice { get; set; }
        public int LeadTimeDays { get; set; } = 0;
        public int MinimumOrderQuantity { get; set; } = 1;
        public bool IsPreferred { get; set; } = false;
        public DateTime ValidFrom { get; set; } = DateTime.UtcNow;
        public DateTime? ValidTo { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateProductVendorDto
    {
        public string? VendorSKU { get; set; }
        public decimal? CostPrice { get; set; }
        public int? LeadTimeDays { get; set; }
        public int? MinimumOrderQuantity { get; set; }
        public bool? IsPreferred { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool? IsActive { get; set; }
    }
}
