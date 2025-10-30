using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Models.Authentication;

namespace ReactLiveSoldProject.ServerBL.Models.Inventory
{
    public class Tag
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El ID de la organización es obligatorio")]
        public Guid OrganizationId { get; set; }

        public virtual Organization Organization { get; set; }

        [Required(ErrorMessage = "El nombre del tag es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre del tag no puede exceder los 100 caracteres")]
        public string Name { get; set; }

        // Propiedad de navegación para la relación M-a-M
        public virtual ICollection<ProductTag> ProductLinks { get; set; } = new List<ProductTag>();
    }

}
