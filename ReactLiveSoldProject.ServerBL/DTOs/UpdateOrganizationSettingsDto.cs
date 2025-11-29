using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Base;

namespace ReactLiveSoldProject.ServerBL.DTOs
{
    /// <summary>
    /// DTO para que usuarios regulares actualicen la configuración de su organización
    /// No incluye campos sensibles como PlanType que solo SuperAdmin puede modificar
    /// </summary>
    public class UpdateOrganizationSettingsDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        [Required(ErrorMessage = "El email de contacto es obligatorio")]
        [EmailAddress]
        [MaxLength(255)]
        public string PrimaryContactEmail { get; set; } = string.Empty;

        public string? CustomizationSettings { get; set; }

        /// <summary>
        /// Método de costeo de inventario (FIFO o AverageCost)
        /// </summary>
        public CostMethod? CostMethod { get; set; }
    }
}
