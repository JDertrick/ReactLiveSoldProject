namespace ReactLiveSoldProject.ServerBL.DTOs.Purchases
{
    public class PaymentTermsDto
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string Description { get; set; }
        public int DueDays { get; set; }
        public decimal DiscountPercentage { get; set; }
        public int DiscountDays { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreatePaymentTermsDto
    {
        public string Description { get; set; }
        public int DueDays { get; set; } = 0;
        public decimal DiscountPercentage { get; set; } = 0;
        public int DiscountDays { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }

    public class UpdatePaymentTermsDto
    {
        public string? Description { get; set; }
        public int? DueDays { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public int? DiscountDays { get; set; }
        public bool? IsActive { get; set; }
    }
}
