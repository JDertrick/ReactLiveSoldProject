using Microsoft.EntityFrameworkCore;
using Mapster;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs.Banking;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Banking;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    /// <summary>
    /// 🏦 CompanyBankAccountService - Gestiona cuentas bancarias de la empresa
    /// </summary>
    public class CompanyBankAccountService : ICompanyBankAccountService
    {
        private readonly LiveSoldDbContext _context;

        public CompanyBankAccountService(LiveSoldDbContext context)
        {
            _context = context;
        }

        public async Task<List<CompanyBankAccountDto>> GetCompanyBankAccountsAsync(Guid organizationId)
        {
            var accounts = await _context.CompanyBankAccounts
                .Include(cba => cba.GLAccount)
                .Where(cba => cba.OrganizationId == organizationId && cba.IsActive)
                .OrderBy(cba => cba.BankName)
                .ThenBy(cba => cba.AccountNumber)
                .ToListAsync();

            return accounts.Adapt<List<CompanyBankAccountDto>>();
        }

        public async Task<CompanyBankAccountDto?> GetCompanyBankAccountByIdAsync(Guid accountId, Guid organizationId)
        {
            var account = await _context.CompanyBankAccounts
                .Include(cba => cba.GLAccount)
                .FirstOrDefaultAsync(cba => cba.Id == accountId && cba.OrganizationId == organizationId);

            return account.Adapt<CompanyBankAccountDto>();
        }

        public async Task<CompanyBankAccountDto> CreateCompanyBankAccountAsync(
            Guid organizationId,
            CreateCompanyBankAccountDto dto)
        {
            // Validar que la cuenta contable existe
            var glAccount = await _context.ChartOfAccounts
                .FirstOrDefaultAsync(coa => coa.Id == dto.GLAccountId && coa.OrganizationId == organizationId);

            if (glAccount == null)
                throw new Exception($"Cuenta contable con ID {dto.GLAccountId} no encontrada");

            var account = new CompanyBankAccount
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                BankName = dto.BankName,
                AccountNumber = dto.AccountNumber,
                Currency = dto.Currency,
                CurrentBalance = dto.CurrentBalance,
                GLAccountId = dto.GLAccountId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.CompanyBankAccounts.Add(account);
            await _context.SaveChangesAsync();

            return await GetCompanyBankAccountByIdAsync(account.Id, organizationId)
                ?? throw new Exception("Error al recuperar la cuenta bancaria creada");
        }

        public async Task<CompanyBankAccountDto> UpdateCompanyBankAccountAsync(
            Guid accountId,
            Guid organizationId,
            UpdateCompanyBankAccountDto dto)
        {
            var account = await _context.CompanyBankAccounts
                .FirstOrDefaultAsync(cba => cba.Id == accountId && cba.OrganizationId == organizationId);

            if (account == null)
                throw new Exception($"Cuenta bancaria con ID {accountId} no encontrada");

            if (dto.BankName != null)
                account.BankName = dto.BankName;
            if (dto.AccountNumber != null)
                account.AccountNumber = dto.AccountNumber;
            if (dto.Currency != null)
                account.Currency = dto.Currency;
            if (dto.IsActive.HasValue)
                account.IsActive = dto.IsActive.Value;

            account.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetCompanyBankAccountByIdAsync(account.Id, organizationId)
                ?? throw new Exception("Error al recuperar la cuenta bancaria actualizada");
        }

        public async Task DeleteCompanyBankAccountAsync(Guid accountId, Guid organizationId)
        {
            var account = await _context.CompanyBankAccounts
                .FirstOrDefaultAsync(cba => cba.Id == accountId && cba.OrganizationId == organizationId);

            if (account == null)
                throw new Exception($"Cuenta bancaria con ID {accountId} no encontrada");

            // Soft delete
            account.IsActive = false;
            account.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}
