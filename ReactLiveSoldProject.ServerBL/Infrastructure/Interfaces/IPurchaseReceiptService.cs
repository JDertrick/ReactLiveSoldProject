using ReactLiveSoldProject.ServerBL.DTOs.Purchases;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface IPurchaseReceiptService
    {
        Task<List<PurchaseReceiptDto>> GetPurchaseReceiptsAsync(Guid organizationId, string? searchTerm = null, string? status = null);
        Task<PurchaseReceiptDto?> GetPurchaseReceiptByIdAsync(Guid receiptId, Guid organizationId);
        Task<PurchaseReceiptDto> CreatePurchaseReceiptAsync(Guid organizationId, Guid userId, CreatePurchaseReceiptDto dto);

        /// <summary>
        /// MÉTODO CRÍTICO: Recibe la mercancía y genera automáticamente:
        /// 1. StockMovement para cada item
        /// 2. StockBatch para FIFO costing
        /// 3. JournalEntry automático (Inventario DEBE / Cuentas por Pagar HABER)
        /// </summary>
        Task<PurchaseReceiptDto> ReceivePurchaseAsync(Guid organizationId, Guid userId, ReceivePurchaseDto dto);

        Task DeletePurchaseReceiptAsync(Guid receiptId, Guid organizationId);
    }
}
