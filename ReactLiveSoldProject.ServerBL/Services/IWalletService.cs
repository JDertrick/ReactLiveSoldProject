using ReactLiveSoldProject.ServerBL.DTOs;

namespace ReactLiveSoldProject.ServerBL.Services
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
    }
}
