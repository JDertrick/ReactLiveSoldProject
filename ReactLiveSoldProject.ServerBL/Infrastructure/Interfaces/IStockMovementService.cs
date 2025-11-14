using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface IStockMovementService
    {
        /// <summary>
        /// Obtiene el historial de movimientos de una variante específica
        /// </summary>
        Task<List<StockMovementDto>> GetMovementsByVariantAsync(Guid productVariantId, Guid organizationId);

        /// <summary>
        /// Obtiene todos los movimientos de inventario de una organización
        /// </summary>
        Task<List<StockMovementDto>> GetMovementsByOrganizationAsync(Guid organizationId, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Crea un movimiento de inventario manualmente (ajustes, compras, pérdidas, etc.)
        /// </summary>
        Task<StockMovementDto> CreateMovementAsync(Guid organizationId, Guid userId, CreateStockMovementDto dto);

        /// <summary>
        /// Registra un movimiento de venta (llamado internamente desde SalesOrderService)
        /// </summary>
        Task RegisterSaleMovementAsync(Guid organizationId, Guid userId, Guid productVariantId, int quantity, Guid salesOrderId);

        /// <summary>
        /// Registra una cancelación de venta (devuelve stock)
        /// </summary>
        Task RegisterSaleCancellationMovementAsync(Guid organizationId, Guid userId, Guid productVariantId, int quantity, Guid salesOrderId);

        /// <summary>
        /// Obtiene el balance actual de stock de una variante (último movimiento)
        /// </summary>
        Task<int> GetCurrentStockAsync(Guid productVariantId, Guid organizationId);
    }
}
