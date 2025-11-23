using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs.Accounting;
using ReactLiveSoldProject.ServerBL.Models.Accounting;
using ReactLiveSoldProject.ServerBL.Models.Inventory;
using ReactLiveSoldProject.ServerBL.Models.Sales;
using System;
using System.Collections.Generic; // Add this using directive
using System.Threading.Tasks;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface IAccountingService
    {
        Task<JournalEntryDto> CreateJournalEntryAsync(Guid organizationId, CreateJournalEntryDto createDto);
        Task<List<ChartOfAccountDto>> GetChartOfAccountsAsync(Guid organizationId);
        Task<ChartOfAccountDto> CreateChartOfAccountAsync(Guid organizationId, CreateChartOfAccountDto createDto);
        Task<List<JournalEntryDto>> GetJournalEntriesAsync(Guid organizationId, DateTime? fromDate = null, DateTime? toDate = null);

        Task RegisterPurchaseAsync(Guid organizationId, StockMovement stockMovement);

        Task RegisterSaleAsync(Guid organizationId, SalesOrder salesOrder);
    }
}
