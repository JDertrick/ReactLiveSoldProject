using ReactLiveSoldProject.ServerBL.Models.Authentication;

namespace ReactLiveSoldProject.ServerBL.Models.Inventory
{
    public class Product
    {
        public Guid Id { get; set; }
        
        public Guid OrganizationId { get; set; }
        
        public virtual Organization Organization { get; set; }
        
        public string Name { get; set; }
        
        public string? Description { get; set; }
        
        public string ProductType { get; set; } = "simple"; // "simple" o "variable"
        
        public bool IsPublished { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación
        public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        
        public virtual ICollection<ProductTag> TagLinks { get; set; } = new List<ProductTag>();
    }
}
