namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class CustomerProfileDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public Guid OrganizationId { get; set; }
    }
}
