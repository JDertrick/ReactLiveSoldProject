namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class VendorDto
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid ContactId { get; set; }
        public ContactDto? Contact { get; set; }
        public Guid? AssignedBuyerId { get; set; }
        public string? AssignedBuyerName { get; set; }
        public string? VendorCode { get; set; }
        public string? Notes { get; set; }
        public string? PaymentTerms { get; set; }
        public decimal CreditLimit { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
