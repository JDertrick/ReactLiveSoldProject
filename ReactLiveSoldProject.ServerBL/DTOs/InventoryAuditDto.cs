using ReactLiveSoldProject.ServerBL.Base;

namespace ReactLiveSoldProject.ServerBL.DTOs
{


    /// <summary>
    /// DTO principal de auditoría de inventario
    /// </summary>
    public class InventoryAuditDto
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime SnapshotTakenAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string? CreatedByUserName { get; set; }
        public Guid? CompletedByUserId { get; set; }
        public string? CompletedByUserName { get; set; }
        public int TotalVariants { get; set; }
        public int CountedVariants { get; set; }
        public int TotalVariance { get; set; }
        public decimal TotalVarianceValue { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Información de scope
        public string ScopeType { get; set; } = "Total";
        public Guid? LocationId { get; set; }
        public string? LocationName { get; set; }
        public string? ScopeDescription { get; set; }

        // Progreso del conteo en porcentaje
        public double Progress => TotalVariants > 0 ? (double)CountedVariants / TotalVariants * 100 : 0;
    }

    /// <summary>
    /// DTO detallado de auditoría con items
    /// </summary>
    public class InventoryAuditDetailDto : InventoryAuditDto
    {
        public List<InventoryAuditItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// DTO de item de auditoría
    /// </summary>
    public class InventoryAuditItemDto
    {
        public Guid Id { get; set; }
        public Guid InventoryAuditId { get; set; }
        public Guid ProductVariantId { get; set; }

        // Información del producto/variante
        public string ProductName { get; set; } = string.Empty;
        public string VariantSku { get; set; } = string.Empty;
        public string? VariantSize { get; set; }
        public string? VariantColor { get; set; }
        public string? VariantImageUrl { get; set; }

        // Datos de snapshot y conteo
        public int TheoreticalStock { get; set; }
        public int? CountedStock { get; set; }
        public int? Variance { get; set; }
        public decimal? VarianceValue { get; set; }
        public decimal SnapshotAverageCost { get; set; }

        // Información del conteo
        public Guid? CountedByUserId { get; set; }
        public string? CountedByUserName { get; set; }
        public DateTime? CountedAt { get; set; }

        // Ajuste generado
        public Guid? AdjustmentMovementId { get; set; }
        public string? Notes { get; set; }

        // Estado del item
        public bool IsCounted => CountedStock.HasValue;
        public bool HasVariance => Variance.HasValue && Variance.Value != 0;
    }

    /// <summary>
    /// DTO para conteo ciego (sin stock teórico)
    /// </summary>
    public class BlindCountItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductVariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string VariantSku { get; set; } = string.Empty;
        public string? VariantSize { get; set; }
        public string? VariantColor { get; set; }
        public string? VariantImageUrl { get; set; }
        public int? CountedStock { get; set; }
        public bool IsCounted => CountedStock.HasValue;
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO para actualizar el conteo de un item
    /// </summary>
    public class UpdateAuditCountDto
    {
        public Guid ItemId { get; set; }
        public int CountedStock { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO para actualizar múltiples conteos a la vez
    /// </summary>
    public class BulkUpdateAuditCountDto
    {
        public List<UpdateAuditCountDto> Counts { get; set; } = new();
    }

    /// <summary>
    /// DTO de resumen de auditoría
    /// </summary>
    public class InventoryAuditSummaryDto
    {
        public Guid AuditId { get; set; }
        public string AuditName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int TotalVariants { get; set; }
        public int CountedVariants { get; set; }
        public int PendingVariants { get; set; }
        public double Progress { get; set; }

        // Varianzas
        public int ItemsWithVariance { get; set; }
        public int ItemsWithPositiveVariance { get; set; }  // Sobrantes
        public int ItemsWithNegativeVariance { get; set; }  // Faltantes
        public int ItemsWithNoVariance { get; set; }        // Sin diferencia

        // Totales de varianza
        public int TotalPositiveVariance { get; set; }      // Total unidades sobrantes
        public int TotalNegativeVariance { get; set; }      // Total unidades faltantes
        public int NetVariance { get; set; }                // Varianza neta

        // Valores monetarios
        public decimal TotalPositiveVarianceValue { get; set; }
        public decimal TotalNegativeVarianceValue { get; set; }
        public decimal NetVarianceValue { get; set; }
    }

    /// <summary>
    /// DTO para completar una auditoría
    /// </summary>
    public class CompleteAuditDto
    {
        /// <summary>
        /// Si es true, los ajustes se postean automáticamente
        /// Si es false, se crean como borradores
        /// </summary>
        public bool AutoPostAdjustments { get; set; } = false;
        public string? Notes { get; set; }
    }
}
