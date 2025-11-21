using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.Models.Authentication;

namespace ReactLiveSoldProject.ServerBL.Models.Inventory
{
    /// <summary>
    /// Representa una auditoría/toma física de inventario
    /// Captura un snapshot del inventario y permite el conteo ciego
    /// </summary>
    public class InventoryAudit
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El ID de la organización es obligatorio")]
        public Guid OrganizationId { get; set; }

        public virtual Organization Organization { get; set; } = null!;

        [Required(ErrorMessage = "El nombre de la auditoría es obligatorio")]
        [MaxLength(200, ErrorMessage = "El nombre no puede exceder los 200 caracteres")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        public string? Description { get; set; }

        /// <summary>
        /// Estado actual de la auditoría
        /// </summary>
        [Required]
        public InventoryAuditStatus Status { get; set; } = InventoryAuditStatus.Draft;

        /// <summary>
        /// Fecha y hora en que se tomó el snapshot del inventario
        /// </summary>
        public DateTime SnapshotTakenAt { get; set; }

        /// <summary>
        /// Fecha y hora en que se inició el conteo
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// Fecha y hora en que se completó la auditoría
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Usuario que creó la auditoría
        /// </summary>
        [Required]
        public Guid CreatedByUserId { get; set; }

        public virtual User CreatedByUser { get; set; } = null!;

        /// <summary>
        /// Usuario que completó la auditoría (aplicó los ajustes)
        /// </summary>
        public Guid? CompletedByUserId { get; set; }

        public virtual User? CompletedByUser { get; set; }

        /// <summary>
        /// Total de variantes incluidas en la auditoría
        /// </summary>
        public int TotalVariants { get; set; }

        /// <summary>
        /// Cantidad de variantes ya contadas
        /// </summary>
        public int CountedVariants { get; set; }

        /// <summary>
        /// Varianza total en unidades (suma de todas las diferencias)
        /// </summary>
        public int TotalVariance { get; set; }

        /// <summary>
        /// Valor monetario de la varianza (basado en costo promedio)
        /// </summary>
        public decimal TotalVarianceValue { get; set; }

        [MaxLength(1000, ErrorMessage = "Las notas no pueden exceder los 1000 caracteres")]
        public string? Notes { get; set; }

        // Campos de Scope/Alcance
        /// <summary>
        /// Tipo de alcance: Total o Partial
        /// </summary>
        [MaxLength(50)]
        public string ScopeType { get; set; } = "Total";

        /// <summary>
        /// Bodega/Ubicación de la auditoría (opcional)
        /// </summary>
        public Guid? LocationId { get; set; }

        public virtual Location? Location { get; set; }

        /// <summary>
        /// Descripción del scope (ej: "Categorías: Damas, Caballeros | Tags: Nike")
        /// </summary>
        [MaxLength(500)]
        public string? ScopeDescription { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navegación
        public virtual ICollection<InventoryAuditItem> Items { get; set; } = new List<InventoryAuditItem>();
    }
}
