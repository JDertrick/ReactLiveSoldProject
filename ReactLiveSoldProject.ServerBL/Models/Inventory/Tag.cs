using ReactLiveSoldProject.ServerBL.Models.Authentication;

namespace ReactLiveSoldProject.ServerBL.Models.Inventory
{
    public class Tag
    {
        public Guid Id { get; set; }

        public Guid OrganizationId { get; set; }
        
        public virtual Organization Organization { get; set; }
        
        public string Name { get; set; }

        // Propiedad de navegación para la relación M-a-M
        public virtual ICollection<ProductTag> ProductLinks { get; set; } = new List<ProductTag>();
    }

}
