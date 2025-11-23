using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs.Accounting;
using ReactLiveSoldProject.ServerBL.Models.Accounting;
using ReactLiveSoldProject.ServerBL.Models.Inventory;
using ReactLiveSoldProject.ServerBL.Models.Sales;
using System;
using System.Threading.Tasks;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface IAccountingService
    {
        Task<JournalEntryDto> CreateJournalEntryAsync(Guid organizationId, CreateJournalEntryDto createDto);

        Task RegisterPurchaseAsync(Guid organizationId, StockMovement stockMovement);

        Task RegisterSaleAsync(Guid organizationId, SalesOrder salesOrder);
    }
}
