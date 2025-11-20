using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Models.Authentication;

namespace ReactLiveSoldProject.ServerBL.Models.Taxes
{
    /// <summary>
    /// Representa una tasa de impuesto configurable para una organización
    /// </summary>
    public class TaxRate
    {
        /// <summary>
        /// ID único de la tasa de impuesto
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID de la organización a la que pertenece
        /// </summary>
        public Guid OrganizationId { get; set; }

        /// <summary>
        /// Nombre descriptivo de la tasa (ej: "IVA 19%", "IVA Reducido 5%")
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Porcentaje de impuesto (ej: 0.19 para 19%, 0.05 para 5%)
        /// </summary>
        [Required]
        [Range(0, 1, ErrorMessage = "La tasa debe estar entre 0 y 1 (0% - 100%)")]
        public decimal Rate { get; set; }

        /// <summary>
        /// Indica si esta es la tasa por defecto que se aplica automáticamente
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// Indica si esta tasa está activa y puede usarse
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Descripción o notas adicionales sobre esta tasa
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Fecha de creación
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Fecha de última actualización
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Fecha desde la cual esta tasa es válida (para histórico)
        /// </summary>
        public DateTime? EffectiveFrom { get; set; }

        /// <summary>
        /// Fecha hasta la cual esta tasa es válida (para histórico)
        /// </summary>
        public DateTime? EffectiveTo { get; set; }

        // Relaciones
        public virtual Organization Organization { get; set; }
    }
}
