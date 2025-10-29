namespace ReactLiveSoldProject.ServerBL.Models.Authentication
{
    public class User
    {
        public Guid Id { get; set; }

        public string? FirstName { get; set; }
        
        public string? LastName { get; set; }
        
        public string Email { get; set; }
        
        public string PasswordHash { get; set; }
        
        public bool IsSuperAdmin { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
