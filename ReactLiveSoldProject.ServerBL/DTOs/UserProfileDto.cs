namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Role { get; set; }
        public Guid? OrganizationId { get; set; }
        public string? OrganizationSlug { get; set; }
        public string? OrganizationName { get; set; }
        public string? OrganizationLogoUrl { get; set; }
        public bool IsSuperAdmin { get; set; }
    }
}
