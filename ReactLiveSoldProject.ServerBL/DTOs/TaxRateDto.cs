namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class TaxRateDto
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string Name { get; set; }
        public decimal Rate { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
