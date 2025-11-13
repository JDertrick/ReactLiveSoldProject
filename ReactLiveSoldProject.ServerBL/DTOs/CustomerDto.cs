namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class CustomerDto
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public Guid? AssignedSellerId { get; set; }
        public string? AssignedSellerName { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public WalletDto? Wallet { get; set; }
        public bool IsActive { get; set; }
    }
}
