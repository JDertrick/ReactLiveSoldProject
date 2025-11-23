using AutoMapper;
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

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    public class AccountingService : IAccountingService
    {
        private readonly LiveSoldDbContext _context;
        private readonly IMapper _mapper;

        public AccountingService(LiveSoldDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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

            var journalEntry = _mapper.Map<JournalEntry>(createDto);
            journalEntry.OrganizationId = organizationId;

            await _context.JournalEntries.AddAsync(journalEntry);
            await _context.SaveChangesAsync();

            return _mapper.Map<JournalEntryDto>(journalEntry);
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
                EntryDate = salesOrder.UpdatedAt,
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
                    EntryDate = salesOrder.UpdatedAt,
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
