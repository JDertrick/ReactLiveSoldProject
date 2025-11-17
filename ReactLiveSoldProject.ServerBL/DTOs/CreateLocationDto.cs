namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class CreateLocationDto
    {
        public Guid OrganizationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
