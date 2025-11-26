using ReactLiveSoldProject.ServerBL.DTOs.Purchases;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface IPurchaseOrderService
    {
        Task<List<PurchaseOrderDto>> GetPurchaseOrdersAsync(Guid organizationId, Guid? vendorId = null, string? status = null);
        Task<PurchaseOrderDto?> GetPurchaseOrderByIdAsync(Guid orderId, Guid organizationId);
        Task<PurchaseOrderDto> CreatePurchaseOrderAsync(Guid organizationId, Guid userId, CreatePurchaseOrderDto dto);
        Task<PurchaseOrderDto> UpdatePurchaseOrderStatusAsync(Guid orderId, Guid organizationId, string status);
        Task DeletePurchaseOrderAsync(Guid orderId, Guid organizationId);
    }
}
