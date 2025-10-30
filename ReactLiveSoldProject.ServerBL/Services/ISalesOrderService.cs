using ReactLiveSoldProject.ServerBL.DTOs;

namespace ReactLiveSoldProject.ServerBL.Services
{
    public interface ISalesOrderService
    {
        /// <summary>
        /// Obtiene todas las órdenes de una organización
        /// </summary>
        Task<List<SalesOrderDto>> GetSalesOrdersByOrganizationAsync(Guid organizationId, string? status = null);

        /// <summary>
        /// Obtiene una orden por ID
        /// </summary>
        Task<SalesOrderDto?> GetSalesOrderByIdAsync(Guid salesOrderId, Guid organizationId);

        /// <summary>
        /// Obtiene todas las órdenes de un cliente
        /// </summary>
        Task<List<SalesOrderDto>> GetSalesOrdersByCustomerIdAsync(Guid customerId, Guid organizationId);

        /// <summary>
        /// Crea una nueva orden en estado Draft
        /// </summary>
        Task<SalesOrderDto> CreateDraftOrderAsync(Guid organizationId, Guid createdByUserId, CreateSalesOrderDto dto);

        /// <summary>
        /// Agrega un item a una orden Draft
        /// </summary>
        Task<SalesOrderDto> AddItemToOrderAsync(Guid salesOrderId, Guid organizationId, CreateSalesOrderItemDto dto);

        /// <summary>
        /// Elimina un item de una orden Draft
        /// </summary>
        Task<SalesOrderDto> RemoveItemFromOrderAsync(Guid salesOrderId, Guid itemId, Guid organizationId);

        /// <summary>
        /// Finaliza una orden Draft: descuenta stock, descuenta wallet, cambia estado a Completed
        /// </summary>
        Task<SalesOrderDto> FinalizeOrderAsync(Guid salesOrderId, Guid organizationId);

        /// <summary>
        /// Cancela una orden (solo Draft)
        /// </summary>
        Task<SalesOrderDto> CancelOrderAsync(Guid salesOrderId, Guid organizationId);
    }
}
