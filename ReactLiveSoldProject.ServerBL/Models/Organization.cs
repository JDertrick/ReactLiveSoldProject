namespace ReactLiveSoldProject.ServerBL.Models
{
    public class Organization
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        
        public string? LogoUrl { get; set; }
        
        public string PrimaryContactEmail { get; set; }
        
        public string PlanType { get; set; } = "standard";
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
