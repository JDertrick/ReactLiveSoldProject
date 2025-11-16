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

        public async Task<ReceiptDto> CreateReceiptAsync(Guid organizationId, Guid authorizedByUserId, CreateReceiptDto dto)
        {
            // 1. Validations
            var customer = await _dbContext.Customers
                .FirstOrDefaultAsync(c => c.Id == dto.CustomerId && c.OrganizationId == organizationId);

            if (customer == null)
                throw new KeyNotFoundException("Cliente no encontrado");

            var user = await _dbContext.Users.FindAsync(authorizedByUserId);
            if (user == null || !await _dbContext.OrganizationMembers.AnyAsync(om => om.OrganizationId == organizationId && om.UserId == authorizedByUserId))
                throw new UnauthorizedAccessException("El usuario no está autorizado para realizar esta acción.");

            // 2. Calculate Total Amount
            var totalAmount = dto.Items.Sum(i => i.UnitPrice * i.Quantity);
            if (totalAmount <= 0)
                throw new InvalidOperationException("El monto total del recibo debe ser mayor a cero.");

            // 3. Create Receipt and Items in Draft state
            var receipt = new Receipt
            {
                OrganizationId = organizationId,
                CustomerId = dto.CustomerId,
                Type = dto.Type,
                TotalAmount = totalAmount,
                Notes = dto.Notes,
                CreatedByUserId = authorizedByUserId,
                IsPosted = false, // Saved as draft
                Items = dto.Items.Select(i => new ReceiptItem
                {
                    Description = i.Description,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    Subtotal = i.UnitPrice * i.Quantity
                }).ToList()
            };

            _dbContext.Receipts.Add(receipt);
            await _dbContext.SaveChangesAsync();

            // 4. Return DTO
            return new ReceiptDto
            {
                Id = receipt.Id,
                OrganizationId = receipt.OrganizationId,
                CustomerId = receipt.CustomerId,
                CustomerName = $"{customer.FirstName} {customer.LastName}".Trim(),
                WalletTransactionId = receipt.WalletTransactionId,
                Type = receipt.Type,
                TotalAmount = receipt.TotalAmount,
                Notes = receipt.Notes,
                CreatedByUserId = receipt.CreatedByUserId,
                CreatedByUserName = $"{user.FirstName} {user.LastName}".Trim(),
                CreatedAt = receipt.CreatedAt,
                IsPosted = receipt.IsPosted,
                Items = receipt.Items.Select(i => new ReceiptItemDto
                {
                    Id = i.Id,
                    Description = i.Description,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    Subtotal = i.Subtotal
                }).ToList()
            };
        }

        public async Task<List<ReceiptDto>> GetReceiptsByCustomerIdAsync(Guid customerId, Guid organizationId)
        {
            var receipts = await _dbContext.Receipts
                .Include(r => r.Customer)
                .Include(r => r.CreatedByUser)
                .Include(r => r.PostedByUser)
                .Include(r => r.RejectedByUser)
                .Include(r => r.Items)
                .Where(r => r.CustomerId == customerId && r.OrganizationId == organizationId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return receipts.Select(receipt => new ReceiptDto
            {
                Id = receipt.Id,
                OrganizationId = receipt.OrganizationId,
                CustomerId = receipt.CustomerId,
                CustomerName = $"{receipt.Customer.FirstName} {receipt.Customer.LastName}".Trim(),
                WalletTransactionId = receipt.WalletTransactionId,
                Type = receipt.Type,
                TotalAmount = receipt.TotalAmount,
                Notes = receipt.Notes,
                CreatedByUserId = receipt.CreatedByUserId,
                CreatedByUserName = $"{receipt.CreatedByUser.FirstName} {receipt.CreatedByUser.LastName}".Trim(),
                CreatedAt = receipt.CreatedAt,
                IsPosted = receipt.IsPosted,
                PostedAt = receipt.PostedAt,
                PostedByUserName = receipt.PostedByUser != null ? $"{receipt.PostedByUser.FirstName} {receipt.PostedByUser.LastName}".Trim() : null,
                IsRejected = receipt.IsRejected,
                RejectedAt = receipt.RejectedAt,
                RejectedByUserName = receipt.RejectedByUser != null ? $"{receipt.RejectedByUser.FirstName} {receipt.RejectedByUser.LastName}".Trim() : null,
                Items = receipt.Items.Select(i => new ReceiptItemDto
                {
                    Id = i.Id,
                    Description = i.Description,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    Subtotal = i.Subtotal
                }).ToList()
            }).ToList();
        }

        public async Task<ReceiptDto> PostReceiptAsync(Guid receiptId, Guid organizationId, Guid userId)
        {
            await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var receipt = await _dbContext.Receipts
                    .Include(r => r.Customer)
                    .ThenInclude(c => c.Wallet)
                    .Include(r => r.CreatedByUser)
                    .Include(r => r.Items)
                    .FirstOrDefaultAsync(r => r.Id == receiptId && r.OrganizationId == organizationId);

                if (receipt == null)
                    throw new KeyNotFoundException("Recibo no encontrado.");

                if (receipt.IsPosted)
                    throw new InvalidOperationException("El recibo ya ha sido posteado.");

                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null || !await _dbContext.OrganizationMembers.AnyAsync(om => om.OrganizationId == organizationId && om.UserId == userId))
                    throw new UnauthorizedAccessException("El usuario no está autorizado para realizar esta acción.");

                var wallet = receipt.Customer.Wallet;
                if (wallet == null)
                    throw new InvalidOperationException("El cliente no tiene una wallet asociada.");

                // Create Wallet Transaction
                var walletTransaction = new WalletTransaction
                {
                    OrganizationId = organizationId,
                    WalletId = wallet.Id,
                    Type = receipt.Type,
                    Amount = receipt.TotalAmount,
                    Notes = $"Transacción generada por recibo ID: {receipt.Id}",
                    AuthorizedByUserId = receipt.CreatedByUserId,
                    ReceiptId = receipt.Id,
                    IsPosted = true,
                    PostedAt = DateTime.UtcNow,
                    PostedByUserId = userId,
                    BalanceBefore = wallet.Balance
                };

                // Update Wallet Balance
                if (receipt.Type == TransactionType.Deposit)
                {
                    wallet.Balance += receipt.TotalAmount;
                }
                else // Withdrawal
                {
                    if (wallet.Balance < receipt.TotalAmount)
                        throw new InvalidOperationException("Fondos insuficientes en la wallet.");
                    wallet.Balance -= receipt.TotalAmount;
                }
                wallet.UpdatedAt = DateTime.UtcNow;
                walletTransaction.BalanceAfter = wallet.Balance;

                _dbContext.WalletTransactions.Add(walletTransaction);
                await _dbContext.SaveChangesAsync();

                // Update Receipt status
                receipt.IsPosted = true;
                receipt.PostedAt = walletTransaction.PostedAt;
                receipt.PostedByUserId = userId;
                receipt.WalletTransactionId = walletTransaction.Id;
                
                await _dbContext.SaveChangesAsync();

                await dbTransaction.CommitAsync();

                // Reload PostedByUser to include in DTO
                await _dbContext.Entry(receipt).Reference(r => r.PostedByUser).LoadAsync();

                return new ReceiptDto
                {
                    Id = receipt.Id,
                    OrganizationId = receipt.OrganizationId,
                    CustomerId = receipt.CustomerId,
                    CustomerName = $"{receipt.Customer.FirstName} {receipt.Customer.LastName}".Trim(),
                    WalletTransactionId = receipt.WalletTransactionId,
                    Type = receipt.Type,
                    TotalAmount = receipt.TotalAmount,
                    Notes = receipt.Notes,
                    CreatedByUserId = receipt.CreatedByUserId,
                    CreatedByUserName = $"{receipt.CreatedByUser.FirstName} {receipt.CreatedByUser.LastName}".Trim(),
                    CreatedAt = receipt.CreatedAt,
                    IsPosted = receipt.IsPosted,
                    PostedAt = receipt.PostedAt,
                    PostedByUserName = receipt.PostedByUser != null ? $"{receipt.PostedByUser.FirstName} {receipt.PostedByUser.LastName}".Trim() : null,
                    Items = receipt.Items.Select(i => new ReceiptItemDto
                    {
                        Id = i.Id,
                        Description = i.Description,
                        UnitPrice = i.UnitPrice,
                        Quantity = i.Quantity,
                        Subtotal = i.Subtotal
                    }).ToList()
                };
            }
            catch (Exception)
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ReceiptDto> RejectReceiptAsync(Guid receiptId, Guid organizationId, Guid userId)
        {
            var receipt = await _dbContext.Receipts
                .Include(r => r.Customer)
                .Include(r => r.CreatedByUser)
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == receiptId && r.OrganizationId == organizationId);

            if (receipt == null)
                throw new KeyNotFoundException("Recibo no encontrado.");

            if (receipt.IsPosted || receipt.IsRejected)
                throw new InvalidOperationException("El recibo ya ha sido procesado (posteado o rechazado).");

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null || !await _dbContext.OrganizationMembers.AnyAsync(om => om.OrganizationId == organizationId && om.UserId == userId))
                throw new UnauthorizedAccessException("El usuario no está autorizado para realizar esta acción.");

            receipt.IsRejected = true;
            receipt.RejectedAt = DateTime.UtcNow;
            receipt.RejectedByUserId = userId;

            await _dbContext.SaveChangesAsync();
            
            await _dbContext.Entry(receipt).Reference(r => r.RejectedByUser).LoadAsync();

            return new ReceiptDto
            {
                Id = receipt.Id,
                OrganizationId = receipt.OrganizationId,
                CustomerId = receipt.CustomerId,
                CustomerName = $"{receipt.Customer.FirstName} {receipt.Customer.LastName}".Trim(),
                WalletTransactionId = receipt.WalletTransactionId,
                Type = receipt.Type,
                TotalAmount = receipt.TotalAmount,
                Notes = receipt.Notes,
                CreatedByUserId = receipt.CreatedByUserId,
                CreatedByUserName = $"{receipt.CreatedByUser.FirstName} {receipt.CreatedByUser.LastName}".Trim(),
                CreatedAt = receipt.CreatedAt,
                IsPosted = receipt.IsPosted,
                PostedAt = receipt.PostedAt,
                PostedByUserName = null, // Not posted
                IsRejected = receipt.IsRejected,
                RejectedAt = receipt.RejectedAt,
                RejectedByUserName = receipt.RejectedByUser != null ? $"{receipt.RejectedByUser.FirstName} {receipt.RejectedByUser.LastName}".Trim() : null,
                Items = receipt.Items.Select(i => new ReceiptItemDto
                {
                    Id = i.Id,
                    Description = i.Description,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    Subtotal = i.Subtotal
                }).ToList()
            };
        }

        public async Task<List<ReceiptDto>> GetReceiptsByOrganizationAsync(Guid organizationId, Guid? customerId, string? status, DateTime? fromDate, DateTime? toDate)
        {
            var query = _dbContext.Receipts
                .Include(r => r.Customer)
                .Include(r => r.CreatedByUser)
                .Include(r => r.PostedByUser)
                .Include(r => r.RejectedByUser)
                .Include(r => r.Items)
                .Where(r => r.OrganizationId == organizationId);

            if (customerId.HasValue)
            {
                query = query.Where(r => r.CustomerId == customerId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                switch (status.ToLower())
                {
                    case "draft":
                        query = query.Where(r => !r.IsPosted && !r.IsRejected);
                        break;
                    case "posted":
                        query = query.Where(r => r.IsPosted);
                        break;
                    case "rejected":
                        query = query.Where(r => r.IsRejected);
                        break;
                }
            }

            if (fromDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt <= toDate.Value);
            }

            var receipts = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return receipts.Select(receipt => new ReceiptDto
            {
                Id = receipt.Id,
                OrganizationId = receipt.OrganizationId,
                CustomerId = receipt.CustomerId,
                CustomerName = $"{receipt.Customer.FirstName} {receipt.Customer.LastName}".Trim(),
                WalletTransactionId = receipt.WalletTransactionId,
                Type = receipt.Type,
                TotalAmount = receipt.TotalAmount,
                Notes = receipt.Notes,
                CreatedByUserId = receipt.CreatedByUserId,
                CreatedByUserName = $"{receipt.CreatedByUser.FirstName} {receipt.CreatedByUser.LastName}".Trim(),
                CreatedAt = receipt.CreatedAt,
                IsPosted = receipt.IsPosted,
                PostedAt = receipt.PostedAt,
                PostedByUserName = receipt.PostedByUser != null ? $"{receipt.PostedByUser.FirstName} {receipt.PostedByUser.LastName}".Trim() : null,
                IsRejected = receipt.IsRejected,
                RejectedAt = receipt.RejectedAt,
                RejectedByUserName = receipt.RejectedByUser != null ? $"{receipt.RejectedByUser.FirstName} {receipt.RejectedByUser.LastName}".Trim() : null,
                Items = receipt.Items.Select(i => new ReceiptItemDto
                {
                    Id = i.Id,
                    Description = i.Description,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    Subtotal = i.Subtotal
                }).ToList()
            }).ToList();
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
