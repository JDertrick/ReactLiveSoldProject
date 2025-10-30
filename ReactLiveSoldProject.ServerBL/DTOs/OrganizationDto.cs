using ReactLiveSoldProject.ServerBL.Base;

namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class OrganizationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string PrimaryContactEmail { get; set; } = string.Empty;
        public string PlanType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
