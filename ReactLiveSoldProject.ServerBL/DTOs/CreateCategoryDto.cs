namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class CreateCategoryDto
    {
        public Guid OrganizationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? ParentId { get; set; }
        public List<CategoryDto> Children { get; set; } = new();
    }
}
