using Mapster;
using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs.Accounting;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Accounting;
using ReactLiveSoldProject.ServerBL.Models.Sales;
using ReactLiveSoldProject.ServerBL.Models.Inventory;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic; // Added for List

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    public class AccountingService : IAccountingService
    {
        private readonly LiveSoldDbContext _context;

        public AccountingService(LiveSoldDbContext context)
        {
            _context = context;
        }

        public async Task<List<ChartOfAccountDto>> GetChartOfAccountsAsync(Guid organizationId)
        {
            var accounts = await _context.ChartOfAccounts
                .Where(ca => ca.OrganizationId == organizationId)
                .OrderBy(ca => ca.AccountCode)
                .ToListAsync();

            return accounts.Adapt<List<ChartOfAccountDto>>();
        }

        public async Task<ChartOfAccountDto> CreateChartOfAccountAsync(Guid organizationId, CreateChartOfAccountDto createDto)
        {
            // Validación de unicidad
            var existingAccount = await _context.ChartOfAccounts
                .AnyAsync(ca => ca.OrganizationId == organizationId &&
                               (ca.AccountCode == createDto.AccountCode || ca.AccountName == createDto.AccountName));
            if (existingAccount)
            {
                throw new InvalidOperationException("Ya existe una cuenta con el mismo código o nombre para esta organización.");
            }

            var account = createDto.Adapt<ChartOfAccount>();
            account.OrganizationId = organizationId;

            await _context.ChartOfAccounts.AddAsync(account);
            await _context.SaveChangesAsync();

            return account.Adapt<ChartOfAccountDto>();
        }

        public async Task<ChartOfAccountDto> UpdateChartOfAccountAsync(Guid organizationId, Guid accountId, UpdateChartOfAccountDto updateDto)
        {
            var account = await _context.ChartOfAccounts
                .FirstOrDefaultAsync(ca => ca.Id == accountId && ca.OrganizationId == organizationId);

            if (account == null)
            {
                throw new KeyNotFoundException("La cuenta contable no existe o no pertenece a su organización.");
            }

            // Validar si el nuevo nombre ya existe en otra cuenta
            if (!string.IsNullOrEmpty(updateDto.AccountName) && updateDto.AccountName != account.AccountName)
            {
                var nameExists = await _context.ChartOfAccounts
                    .AnyAsync(ca => ca.OrganizationId == organizationId &&
                                   ca.AccountName == updateDto.AccountName &&
                                   ca.Id != accountId);
                if (nameExists)
                {
                    throw new InvalidOperationException("Ya existe otra cuenta con ese nombre.");
                }
                account.AccountName = updateDto.AccountName;
            }

            if (updateDto.AccountType.HasValue)
            {
                account.AccountType = updateDto.AccountType.Value;
            }

            if (updateDto.Description != null)
            {
                account.Description = updateDto.Description;
            }

            account.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return account.Adapt<ChartOfAccountDto>();
        }

        public async Task DeleteChartOfAccountAsync(Guid organizationId, Guid accountId)
        {
            var account = await _context.ChartOfAccounts
                .FirstOrDefaultAsync(ca => ca.Id == accountId && ca.OrganizationId == organizationId);

            if (account == null)
            {
                throw new KeyNotFoundException("La cuenta contable no existe o no pertenece a su organización.");
            }

            // Verificar si la cuenta está siendo utilizada en asientos contables
            var isUsedInJournalEntries = await _context.JournalEntryLines
                .AnyAsync(jel => jel.AccountId == accountId);

            if (isUsedInJournalEntries)
            {
                throw new InvalidOperationException("No se puede eliminar la cuenta porque está siendo utilizada en asientos contables.");
            }

            // Verificar si tiene cuentas hijas
            var hasChildAccounts = await _context.ChartOfAccounts
                .AnyAsync(ca => ca.ParentAccountId == accountId);

            if (hasChildAccounts)
            {
                throw new InvalidOperationException("No se puede eliminar la cuenta porque tiene cuentas hijas asociadas.");
            }

            _context.ChartOfAccounts.Remove(account);
            await _context.SaveChangesAsync();
        }

        public async Task<List<JournalEntryDto>> GetJournalEntriesAsync(Guid organizationId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.JournalEntries
                .Include(je => je.JournalEntryLines)
                    .ThenInclude(jel => jel.Account) // Cargar la cuenta asociada a cada línea
                .Where(je => je.OrganizationId == organizationId);

            if (fromDate.HasValue)
            {
                query = query.Where(je => je.EntryDate >= fromDate.Value.Date);
            }
            if (toDate.HasValue)
            {
                query = query.Where(je => je.EntryDate <= toDate.Value.Date.AddDays(1).AddTicks(-1)); // Incluir todo el día
            }

            var journalEntries = await query
                .OrderByDescending(je => je.EntryDate)
                .ThenByDescending(je => je.CreatedAt)
                .ToListAsync();

            return journalEntries.Adapt<List<JournalEntryDto>>();
        }

        public async Task<JournalEntryDto> CreateJournalEntryAsync(Guid organizationId, CreateJournalEntryDto createDto)
        {
            // Validación 1: Verificar que los débitos son iguales a los créditos
            var totalDebit = createDto.Lines.Sum(l => l.Debit);
            var totalCredit = createDto.Lines.Sum(l => l.Credit);

            if (totalDebit != totalCredit)
            {
                throw new InvalidOperationException("El total de débitos debe ser igual al total de créditos.");
            }

            // Validación 2: No permitir montos negativos
            if (createDto.Lines.Any(l => l.Debit < 0 || l.Credit < 0))
            {
                throw new InvalidOperationException("Los montos de débito y crédito no pueden ser negativos.");
            }

            // Validación 3: Al menos dos líneas
            if (createDto.Lines.Count < 2)
            {
                throw new InvalidOperationException("Un asiento contable debe tener al menos dos líneas.");
            }

            // Validación 4: Verificar que las AccountId existen y pertenecen a la organización
            var distinctAccountIds = createDto.Lines.Select(l => l.AccountId).Distinct();
            var existingAccounts = await _context.ChartOfAccounts
                .Where(ca => ca.OrganizationId == organizationId && distinctAccountIds.Contains(ca.Id))
                .Select(ca => ca.Id)
                .ToListAsync();

            if (existingAccounts.Count != distinctAccountIds.Count())
            {
                var missingAccountIds = distinctAccountIds.Except(existingAccounts).ToList();
                throw new KeyNotFoundException($"Una o más cuentas contables no existen o no pertenecen a esta organización: {string.Join(", ", missingAccountIds)}");
            }


            var journalEntry = createDto.Adapt<JournalEntry>();
            journalEntry.OrganizationId = organizationId;

            await _context.JournalEntries.AddAsync(journalEntry);
            await _context.SaveChangesAsync();

            // Cargar las líneas y cuentas para el DTO de retorno
            await _context.Entry(journalEntry)
                .Collection(je => je.JournalEntryLines)
                .LoadAsync();
            foreach (var line in journalEntry.JournalEntryLines)
            {
                await _context.Entry(line)
                    .Reference(jel => jel.Account)
                    .LoadAsync();
            }

            return journalEntry.Adapt<JournalEntryDto>();
        }

        public async Task RegisterPurchaseAsync(Guid organizationId, StockMovement stockMovement)
        {
            if (stockMovement.MovementType != StockMovementType.Purchase || !stockMovement.UnitCost.HasValue || stockMovement.UnitCost.Value <= 0)
            {
                // No es una compra con costo, no se contabiliza.
                return;
            }

            // 1. Obtener cuentas de sistema
            var inventoryAccount = await GetSystemAccountAsync(organizationId, SystemAccountType.Inventory);
            var accountsPayableAccount = await GetSystemAccountAsync(organizationId, SystemAccountType.AccountsPayable);

            // 2. Calcular el valor total
            var totalValue = stockMovement.Quantity * stockMovement.UnitCost.Value;

            // 3. Crear el DTO del asiento contable
            var journalEntryDto = new CreateJournalEntryDto
            {
                EntryDate = stockMovement.PostedAt ?? DateTime.UtcNow,
                Description = $"Compra de inventario - SKU: {stockMovement.ProductVariant.Sku}",
                ReferenceNumber = stockMovement.Reference,
                Lines = new System.Collections.Generic.List<CreateJournalEntryLineDto>
                {
                    // Débito a la cuenta de Inventario
                    new CreateJournalEntryLineDto { AccountId = inventoryAccount.Id, Debit = totalValue },
                    // Crédito a la cuenta de Cuentas por Pagar
                    new CreateJournalEntryLineDto { AccountId = accountsPayableAccount.Id, Credit = totalValue }
                }
            };

            // 4. Llamar al método principal para crear el asiento
            await CreateJournalEntryAsync(organizationId, journalEntryDto);
        }

        public async Task RegisterSaleAsync(Guid organizationId, SalesOrder salesOrder)
        {
            // Asiento 1: Registro del Ingreso de la Venta
            var arAccount = await GetSystemAccountAsync(organizationId, SystemAccountType.AccountsReceivable);
            var salesRevenueAccount = await GetSystemAccountAsync(organizationId, SystemAccountType.SalesRevenue);

            var saleEntryDto = new CreateJournalEntryDto
            {
                EntryDate = salesOrder.UpdatedAt, // Use CreatedAt if UpdatedAt is null
                Description = $"Venta - Orden No. {salesOrder.Id.ToString().Substring(0, 8)}",
                ReferenceNumber = salesOrder.Id.ToString(),
                Lines = new System.Collections.Generic.List<CreateJournalEntryLineDto>
                {
                    // Débito a Cuentas por Cobrar por el total de la orden
                    new CreateJournalEntryLineDto { AccountId = arAccount.Id, Debit = salesOrder.TotalAmount },
                    // Crédito a Ingresos por Venta por el subtotal
                    new CreateJournalEntryLineDto { AccountId = salesRevenueAccount.Id, Credit = salesOrder.SubtotalAmount }
                }
            };

            // Añadir línea de impuesto si aplica
            if (salesOrder.TotalTaxAmount > 0)
            {
                var taxPayableAccount = await GetSystemAccountAsync(organizationId, SystemAccountType.SalesTaxPayable);
                saleEntryDto.Lines.Add(new CreateJournalEntryLineDto { AccountId = taxPayableAccount.Id, Credit = salesOrder.TotalTaxAmount });
            }

            await CreateJournalEntryAsync(organizationId, saleEntryDto);

            // Asiento 2: Registro del Costo de la Mercancía Vendida (COGS)
            var cogsAccount = await GetSystemAccountAsync(organizationId, SystemAccountType.CostOfGoodsSold);
            var inventoryAccount = await GetSystemAccountAsync(organizationId, SystemAccountType.Inventory);

            // La orden debe tener los items cargados
            var totalCost = salesOrder.Items.Sum(i => (i.UnitCost) * i.Quantity);

            if (totalCost > 0)
            {
                var cogsEntryDto = new CreateJournalEntryDto
                {
                    EntryDate = salesOrder.UpdatedAt, // Use CreatedAt if UpdatedAt is null
                    Description = $"Costo de Venta - Orden No. {salesOrder.Id.ToString().Substring(0, 8)}",
                    ReferenceNumber = salesOrder.Id.ToString(),
                    Lines = new System.Collections.Generic.List<CreateJournalEntryLineDto>
                    {
                        // Débito a Costo de Mercancía Vendida
                        new CreateJournalEntryLineDto { AccountId = cogsAccount.Id, Debit = totalCost },
                        // Crédito a Inventario
                        new CreateJournalEntryLineDto { AccountId = inventoryAccount.Id, Credit = totalCost }
                    }
                };
                await CreateJournalEntryAsync(organizationId, cogsEntryDto);
            }
        }


        /// <summary>
        /// Obtiene una cuenta de sistema requerida para una organización.
        /// Lanza una excepción si la cuenta no está configurada.
        /// </summary>
        private async Task<ChartOfAccount> GetSystemAccountAsync(Guid organizationId, SystemAccountType systemAccountType)
        {
            var account = await _context.ChartOfAccounts
                .FirstOrDefaultAsync(ca => ca.OrganizationId == organizationId && ca.SystemAccountType == systemAccountType && ca.IsActive);

            if (account == null)
            {
                throw new InvalidOperationException($"La cuenta de sistema requerida '{systemAccountType}' no está configurada o está inactiva para esta organización.");
            }

            return account;
        }
    }
}
