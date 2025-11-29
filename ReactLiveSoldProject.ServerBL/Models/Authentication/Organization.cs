using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.Models.Audit;
using ReactLiveSoldProject.ServerBL.Models.CustomerWallet;
using ReactLiveSoldProject.ServerBL.Models.Inventory;
using ReactLiveSoldProject.ServerBL.Models.Sales;
using ReactLiveSoldProject.ServerBL.Models.Taxes;

namespace ReactLiveSoldProject.ServerBL.Models.Authentication
{
    public class Organization
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El nombre de la organización es obligatorio")]
        [MaxLength(200, ErrorMessage = "El nombre no puede exceder los 200 caracteres")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El slug es obligatorio")]
        [MaxLength(100, ErrorMessage = "El slug no puede exceder los 100 caracteres")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "El slug solo puede contener letras minúsculas, números y guiones")]
        public string Slug { get; set; }

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

        /// <summary>
        /// Configuración de personalización de la interfaz (colores, temas, etc.)
        /// Almacenado como JSON
        /// </summary>
        public string? CustomizationSettings { get; set; }

        // ==================== CONFIGURACIÓN DE IMPUESTOS ====================

        /// <summary>
        /// Indica si el sistema de impuestos está habilitado para esta organización
        /// </summary>
        public bool TaxEnabled { get; set; } = false;

        /// <summary>
        /// Tipo de sistema de impuestos utilizado (IVA, Sales Tax, GST, etc.)
        /// </summary>
        public TaxSystemType TaxSystemType { get; set; } = TaxSystemType.None;

        /// <summary>
        /// Nombre personalizado para mostrar del impuesto (ej: "IVA", "IGV", "Tax")
        /// </summary>
        [MaxLength(50)]
        public string? TaxDisplayName { get; set; }

        /// <summary>
        /// Modo de aplicación del impuesto: Incluido en precio o Excluido (se suma al precio)
        /// </summary>
        public TaxApplicationMode TaxApplicationMode { get; set; } = TaxApplicationMode.TaxIncluded;

        /// <summary>
        /// ID de la tasa de impuesto por defecto (se aplica automáticamente si no se especifica otra)
        /// </summary>
        public Guid? DefaultTaxRateId { get; set; }

        // ==================== CONFIGURACIÓN DE COSTOS ====================

        /// <summary>
        /// Método de costeo de inventario a utilizar para cálculos y reportes
        /// FIFO (First In First Out) o AverageCost (Costo Promedio Ponderado)
        /// Nota: Ambos métodos se calculan siempre, pero este determina cuál se usa en reportes
        /// </summary>
        public CostMethod CostMethod { get; set; } = CostMethod.FIFO;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación
        public virtual ICollection<OrganizationMember> Members { get; set; } = new List<OrganizationMember>();
        
        public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
        
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        
        public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
        
        public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();

        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

        public virtual ICollection<TaxRate> TaxRates { get; set; } = new List<TaxRate>();

    }
}
