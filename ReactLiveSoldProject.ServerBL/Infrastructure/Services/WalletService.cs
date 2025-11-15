using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Authentication;
using ReactLiveSoldProject.ServerBL.Models.CustomerWallet;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    public class WalletService : IWalletService
    {
        private readonly LiveSoldDbContext _dbContext;
        private readonly IMapper _mapper;

        public WalletService(LiveSoldDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<WalletDto?> GetWalletByCustomerIdAsync(Guid customerId, Guid organizationId)
        {
            var wallet = await _dbContext.Wallets
                .Include(w => w.Customer)
                .FirstOrDefaultAsync(w => w.CustomerId == customerId && w.OrganizationId == organizationId);

            if (wallet == null)
                return null;

            return new WalletDto
            {
                Id = wallet.Id,
                CustomerId = wallet.CustomerId,
                CustomerName = $"{wallet.Customer.FirstName} {wallet.Customer.LastName}".Trim(),
                CustomerEmail = wallet.Customer.Email,
                Balance = wallet.Balance,
                UpdatedAt = wallet.UpdatedAt
            };
        }

        public async Task<List<WalletTransactionDto>> GetTransactionsByWalletIdAsync(Guid walletId, Guid organizationId)
        {
            try
            {
                var transactions = await _dbContext.WalletTransactions
                .Include(wt => wt.AuthorizedByUser)
                .Where(wt => wt.WalletId == walletId && wt.OrganizationId == organizationId)
                .OrderByDescending(wt => wt.CreatedAt)
                .ToListAsync();

                return transactions.Select(t => MapToDto(t)).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<WalletTransactionDto>> GetTransactionsByCustomerIdAsync(Guid customerId, Guid organizationId)
        {
            var wallet = await _dbContext.Wallets
                .FirstOrDefaultAsync(w => w.CustomerId == customerId && w.OrganizationId == organizationId);

            if (wallet == null)
                return new List<WalletTransactionDto>();

            return await GetTransactionsByWalletIdAsync(wallet.Id, organizationId);
        }

        public async Task<WalletTransactionDto> CreateDepositAsync(Guid organizationId, Guid authorizedByUserId, CreateWalletTransactionDto dto)
        {
            try
            {
                // Validar TransactionType
                if (!Enum.TryParse<TransactionType>(dto.Type, out var transactionType) || transactionType != TransactionType.Deposit)
                    throw new InvalidOperationException("El tipo de transacción debe ser 'Deposit'");

                // Obtener el customer y su wallet
                var customer = await _dbContext.Customers
                    .Include(c => c.Wallet)
                    .FirstOrDefaultAsync(c => c.Id == dto.CustomerId && c.OrganizationId == organizationId);

                if (customer == null)
                    throw new KeyNotFoundException("Cliente no encontrado");

                if (customer.Wallet == null)
                    throw new InvalidOperationException("El cliente no tiene una wallet asociada");

                // Verificar que el usuario autorizador pertenece a la organización
                var userBelongsToOrg = await _dbContext.OrganizationMembers
                    .AnyAsync(om => om.OrganizationId == organizationId && om.UserId == authorizedByUserId);

                if (!userBelongsToOrg)
                    throw new UnauthorizedAccessException("El usuario no pertenece a esta organización");

                // Crear la transacción
                var transactionId = Guid.NewGuid();

                var transaction = _mapper.Map<WalletTransaction>(dto);
                transaction.OrganizationId = organizationId;
                transaction.WalletId = customer.Wallet.Id;
                transaction.AuthorizedByUserId = authorizedByUserId;

                _dbContext.WalletTransactions.Add(transaction);

                // Actualizar el balance del wallet
                customer.Wallet.Balance += dto.Amount;
                customer.Wallet.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                // Recargar con relaciones
                await _dbContext.Entry(transaction).Reference(t => t.AuthorizedByUser).LoadAsync();

                return _mapper.Map<WalletTransactionDto>(transaction);
            }
            catch (Exception ex)
            {
                throw ex;
            }            
        }

        public async Task<WalletTransactionDto> CreateWithdrawalAsync(Guid organizationId, Guid authorizedByUserId, CreateWalletTransactionDto dto)
        {
            // Validar TransactionType
            if (!Enum.TryParse<TransactionType>(dto.Type, out var transactionType) || transactionType != TransactionType.Withdrawal)
                throw new InvalidOperationException("El tipo de transacción debe ser 'Withdrawal'");

            // Obtener el customer y su wallet
            var customer = await _dbContext.Customers
                .Include(c => c.Wallet)
                .FirstOrDefaultAsync(c => c.Id == dto.CustomerId && c.OrganizationId == organizationId);

            if (customer == null)
                throw new KeyNotFoundException("Cliente no encontrado");

            if (customer.Wallet == null)
                throw new InvalidOperationException("El cliente no tiene una wallet asociada");

            // Verificar que hay fondos suficientes
            if (customer.Wallet.Balance < dto.Amount)
                throw new InvalidOperationException($"Fondos insuficientes. Balance actual: {customer.Wallet.Balance:C}");

            // Verificar que el usuario autorizador pertenece a la organización
            var userBelongsToOrg = await _dbContext.OrganizationMembers
                .AnyAsync(om => om.OrganizationId == organizationId && om.UserId == authorizedByUserId);

            if (!userBelongsToOrg)
                throw new UnauthorizedAccessException("El usuario no pertenece a esta organización");

            // Crear la transacción
            var transaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                WalletId = customer.Wallet.Id,
                Type = TransactionType.Withdrawal,
                Amount = dto.Amount,
                AuthorizedByUserId = authorizedByUserId,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.WalletTransactions.Add(transaction);

            // Actualizar el balance del wallet
            customer.Wallet.Balance -= dto.Amount;
            customer.Wallet.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            // Recargar con relaciones
            await _dbContext.Entry(transaction).Reference(t => t.AuthorizedByUser).LoadAsync();

            return MapToDto(transaction);
        }

        private static WalletTransactionDto MapToDto(WalletTransaction transaction)
        {
            return new WalletTransactionDto
            {
                Id = transaction.Id,
                WalletId = transaction.WalletId,
                Type = transaction.Type.ToString(),
                Amount = transaction.Amount,
                RelatedSalesOrderId = transaction.RelatedSalesOrderId,
                AuthorizedByUserId = transaction.AuthorizedByUserId,
                AuthorizedByUserName = transaction.AuthorizedByUser != null
                    ? $"{transaction.AuthorizedByUser.FirstName} {transaction.AuthorizedByUser.LastName}".Trim()
                    : null,
                Notes = transaction.Notes,
                CreatedAt = transaction.CreatedAt
            };
        }
    }
}
