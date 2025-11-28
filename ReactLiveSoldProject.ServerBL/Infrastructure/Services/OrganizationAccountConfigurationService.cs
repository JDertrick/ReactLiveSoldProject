using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs.Accounting;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Accounting;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    public class OrganizationAccountConfigurationService : IOrganizationAccountConfigurationService
    {
        private readonly LiveSoldDbContext _context;

        public OrganizationAccountConfigurationService(LiveSoldDbContext context)
        {
            _context = context;
        }

        public async Task<OrganizationAccountConfigurationDto?> GetConfigurationAsync(Guid organizationId)
        {
            var config = await _context.OrganizationAccountConfigurations
                .Include(c => c.InventoryAccount)
                .Include(c => c.AccountsPayableAccount)
                .Include(c => c.AccountsReceivableAccount)
                .Include(c => c.SalesRevenueAccount)
                .Include(c => c.CostOfGoodsSoldAccount)
                .Include(c => c.TaxPayableAccount)
                .Include(c => c.TaxReceivableAccount)
                .Include(c => c.CashAccount)
                .Include(c => c.DefaultBankAccount)
                .FirstOrDefaultAsync(c => c.OrganizationId == organizationId);

            if (config == null)
                return null;

            return MapToDto(config);
        }

        public async Task<OrganizationAccountConfigurationDto> CreateOrUpdateConfigurationAsync(
            Guid organizationId,
            UpdateOrganizationAccountConfigurationDto dto)
        {
            var existingConfig = await _context.OrganizationAccountConfigurations
                .FirstOrDefaultAsync(c => c.OrganizationId == organizationId);

            if (existingConfig == null)
            {
                // Crear nueva configuración
                var newConfig = new OrganizationAccountConfiguration
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    InventoryAccountId = dto.InventoryAccountId,
                    AccountsPayableAccountId = dto.AccountsPayableAccountId,
                    AccountsReceivableAccountId = dto.AccountsReceivableAccountId,
                    SalesRevenueAccountId = dto.SalesRevenueAccountId,
                    CostOfGoodsSoldAccountId = dto.CostOfGoodsSoldAccountId,
                    TaxPayableAccountId = dto.TaxPayableAccountId,
                    TaxReceivableAccountId = dto.TaxReceivableAccountId,
                    CashAccountId = dto.CashAccountId,
                    DefaultBankAccountId = dto.DefaultBankAccountId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.OrganizationAccountConfigurations.Add(newConfig);
                await _context.SaveChangesAsync();

                // Reload with includes
                return await GetConfigurationAsync(organizationId) 
                    ?? throw new Exception("Error al crear la configuración");
            }
            else
            {
                // Actualizar configuración existente
                existingConfig.InventoryAccountId = dto.InventoryAccountId;
                existingConfig.AccountsPayableAccountId = dto.AccountsPayableAccountId;
                existingConfig.AccountsReceivableAccountId = dto.AccountsReceivableAccountId;
                existingConfig.SalesRevenueAccountId = dto.SalesRevenueAccountId;
                existingConfig.CostOfGoodsSoldAccountId = dto.CostOfGoodsSoldAccountId;
                existingConfig.TaxPayableAccountId = dto.TaxPayableAccountId;
                existingConfig.TaxReceivableAccountId = dto.TaxReceivableAccountId;
                existingConfig.CashAccountId = dto.CashAccountId;
                existingConfig.DefaultBankAccountId = dto.DefaultBankAccountId;
                existingConfig.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Reload with includes
                return await GetConfigurationAsync(organizationId)
                    ?? throw new Exception("Error al actualizar la configuración");
            }
        }

        private OrganizationAccountConfigurationDto MapToDto(OrganizationAccountConfiguration config)
        {
            return new OrganizationAccountConfigurationDto
            {
                Id = config.Id,
                OrganizationId = config.OrganizationId,
                InventoryAccountId = config.InventoryAccountId,
                InventoryAccountName = config.InventoryAccount?.AccountName,
                AccountsPayableAccountId = config.AccountsPayableAccountId,
                AccountsPayableAccountName = config.AccountsPayableAccount?.AccountName,
                AccountsReceivableAccountId = config.AccountsReceivableAccountId,
                AccountsReceivableAccountName = config.AccountsReceivableAccount?.AccountName,
                SalesRevenueAccountId = config.SalesRevenueAccountId,
                SalesRevenueAccountName = config.SalesRevenueAccount?.AccountName,
                CostOfGoodsSoldAccountId = config.CostOfGoodsSoldAccountId,
                CostOfGoodsSoldAccountName = config.CostOfGoodsSoldAccount?.AccountName,
                TaxPayableAccountId = config.TaxPayableAccountId,
                TaxPayableAccountName = config.TaxPayableAccount?.AccountName,
                TaxReceivableAccountId = config.TaxReceivableAccountId,
                TaxReceivableAccountName = config.TaxReceivableAccount?.AccountName,
                CashAccountId = config.CashAccountId,
                CashAccountName = config.CashAccount?.AccountName,
                DefaultBankAccountId = config.DefaultBankAccountId,
                DefaultBankAccountName = config.DefaultBankAccount?.AccountName,
                CreatedAt = config.CreatedAt,
                UpdatedAt = config.UpdatedAt
            };
        }
    }
}
