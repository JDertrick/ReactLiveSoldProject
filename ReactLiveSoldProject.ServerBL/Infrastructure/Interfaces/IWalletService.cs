using ReactLiveSoldProject.ServerBL.DTOs;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface IWalletService
    {
        /// <summary>
        /// Obtiene el wallet de un cliente
        /// </summary>
        Task<WalletDto?> GetWalletByCustomerIdAsync(Guid customerId, Guid organizationId);

        /// <summary>
        /// Obtiene todas las transacciones de un wallet
        /// </summary>
        Task<List<WalletTransactionDto>> GetTransactionsByWalletIdAsync(Guid walletId, Guid organizationId);

        /// <summary>
        /// Obtiene todas las transacciones de un cliente
        /// </summary>
        Task<List<WalletTransactionDto>> GetTransactionsByCustomerIdAsync(Guid customerId, Guid organizationId);

        /// <summary>
        /// Crea un depósito en la wallet del cliente
        /// </summary>
        Task<WalletTransactionDto> CreateDepositAsync(Guid organizationId, Guid authorizedByUserId, CreateWalletTransactionDto dto);

        /// <summary>
        /// Crea un retiro de la wallet del cliente
        /// </summary>
        Task<WalletTransactionDto> CreateWithdrawalAsync(Guid organizationId, Guid authorizedByUserId, CreateWalletTransactionDto dto);

        /// <summary>
        /// Crea un recibo y la transacción de wallet asociada
        /// </summary>
        Task<ReceiptDto> CreateReceiptAsync(Guid organizationId, Guid authorizedByUserId, CreateReceiptDto dto);

        /// <summary>
        /// Obtiene todos los recibos de un cliente
        /// </summary>
        Task<List<ReceiptDto>> GetReceiptsByCustomerIdAsync(Guid customerId, Guid organizationId);

        /// <summary>
        /// Postea un recibo y crea la transacción de wallet asociada
        /// </summary>
        Task<ReceiptDto> PostReceiptAsync(Guid receiptId, Guid organizationId, Guid userId);
    }
}
