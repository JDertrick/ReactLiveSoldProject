using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Base;

namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class CreateOrganizationDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "El slug solo puede contener letras minúsculas, números y guiones")]
        public string? Slug { get; set; }  // Si es null, se generará automáticamente desde el nombre

        [Url]
        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        [Required(ErrorMessage = "El email de contacto es obligatorio")]
        [EmailAddress]
        [MaxLength(255)]
        public string PrimaryContactEmail { get; set; } = string.Empty;

        public PlanType PlanType { get; set; } = PlanType.Standard;
    }
}
