using ReactLiveSoldProject.ServerBL.DTOs;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface IInventoryAuditService
    {
        /// <summary>
        /// Crea una nueva auditoría y toma el snapshot del inventario actual
        /// </summary>
        Task<InventoryAuditDto> CreateAuditAsync(Guid organizationId, Guid userId, CreateInventoryAuditDto dto);

        /// <summary>
        /// Obtiene una auditoría por su ID
        /// </summary>
        Task<InventoryAuditDetailDto?> GetAuditByIdAsync(Guid auditId, Guid organizationId);

        /// <summary>
        /// Obtiene todas las auditorías de una organización
        /// </summary>
        Task<List<InventoryAuditDto>> GetAuditsByOrganizationAsync(Guid organizationId, string? status = null);

        /// <summary>
        /// Inicia el proceso de conteo (cambia estado a InProgress)
        /// </summary>
        Task<InventoryAuditDto> StartAuditAsync(Guid auditId, Guid organizationId, Guid userId);

        /// <summary>
        /// Obtiene los items para conteo ciego (sin stock teórico)
        /// </summary>
        Task<List<BlindCountItemDto>> GetBlindCountItemsAsync(Guid auditId, Guid organizationId);

        /// <summary>
        /// Obtiene los items con toda la información (para revisión después del conteo)
        /// </summary>
        Task<List<InventoryAuditItemDto>> GetAuditItemsAsync(Guid auditId, Guid organizationId);

        /// <summary>
        /// Actualiza el conteo de un item
        /// </summary>
        Task<InventoryAuditItemDto> UpdateCountAsync(Guid auditId, Guid organizationId, Guid userId, UpdateAuditCountDto dto);

        /// <summary>
        /// Actualiza múltiples conteos a la vez
        /// </summary>
        Task<List<InventoryAuditItemDto>> BulkUpdateCountAsync(Guid auditId, Guid organizationId, Guid userId, BulkUpdateAuditCountDto dto);

        /// <summary>
        /// Obtiene el resumen de la auditoría con estadísticas de varianza
        /// </summary>
        Task<InventoryAuditSummaryDto> GetAuditSummaryAsync(Guid auditId, Guid organizationId);

        /// <summary>
        /// Completa la auditoría y genera los movimientos de ajuste
        /// </summary>
        Task<InventoryAuditDto> CompleteAuditAsync(Guid auditId, Guid organizationId, Guid userId, CompleteAuditDto dto);

        /// <summary>
        /// Cancela una auditoría (solo si no está completada)
        /// </summary>
        Task<InventoryAuditDto> CancelAuditAsync(Guid auditId, Guid organizationId, Guid userId);
    }
}
