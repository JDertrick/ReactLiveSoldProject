using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.Models.Audit;
using ReactLiveSoldProject.ServerBL.Models.CustomerWallet;
using ReactLiveSoldProject.ServerBL.Models.Inventory;
using ReactLiveSoldProject.ServerBL.Models.Sales;

namespace ReactLiveSoldProject.ServerBL.Models.Authentication
{
    public class Organization
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El nombre de la organización es obligatorio")]
        [MaxLength(200, ErrorMessage = "El nombre no puede exceder los 200 caracteres")]
        public string Name { get; set; }

        [Url(ErrorMessage = "La URL del logo debe ser válida")]
        [MaxLength(500, ErrorMessage = "La URL del logo no puede exceder los 500 caracteres")]
        public string? LogoUrl { get; set; }

        [Required(ErrorMessage = "El email de contacto es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [MaxLength(255, ErrorMessage = "El email no puede exceder los 255 caracteres")]
        public string PrimaryContactEmail { get; set; }

        [Required(ErrorMessage = "El tipo de plan es obligatorio")]
        public PlanType PlanType { get; set; } = PlanType.Standard;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación
        public virtual ICollection<OrganizationMember> Members { get; set; } = new List<OrganizationMember>();
        
        public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
        
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        
        public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
        
        public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
        
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    }
}
