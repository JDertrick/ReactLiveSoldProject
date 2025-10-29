namespace ReactLiveSoldProject.ServerBL.Models.Authentication
{
    public class OrganizationMember
    {
        public Guid Id { get; set; }

        
        public Guid OrganizationId { get; set; }
        
        public virtual Organization Organization { get; set; }
        
        public Guid UserId { get; set; }
        
        public virtual User User { get; set; }
        
        public string Role { get; set; } = "seller";
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
