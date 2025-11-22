using ReactLiveSoldProject.ServerBL.Base;
using System.Collections.Generic;
using System;

namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public enum AuditScopeType
    {
        Total,
        Partial,
        Manual // Added for manual selection
    }

    public class CreateInventoryAuditDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Notes { get; set; }
        public AuditScopeType ScopeType { get; set; } = AuditScopeType.Total;
        public Guid? LocationId { get; set; }
        public List<Guid>? CategoryIds { get; set; }
        public List<Guid>? TagIds { get; set; }
        public List<Guid>? ProductVariantIds { get; set; } // For manual selection
        public int RandomSampleCount { get; set; } = 0;
        public int ExcludeAuditedInLastDays { get; set; } = 0;
    }
}
