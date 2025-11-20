using System.ComponentModel.DataAnnotations;

namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class UpdateTaxRateDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "La tasa es requerida")]
        [Range(0, 1, ErrorMessage = "La tasa debe estar entre 0 y 1 (0% - 100%)")]
        public decimal Rate { get; set; }

        public bool IsDefault { get; set; }

        public bool IsActive { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime? EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }
    }
}
