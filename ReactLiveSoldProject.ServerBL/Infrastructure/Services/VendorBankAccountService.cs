using Microsoft.EntityFrameworkCore;
using AutoMapper;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs.Vendors;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Vendors;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    /// <summary>
    /// 🏦 VendorBankAccountService - Gestiona cuentas bancarias de proveedores
    /// </summary>
    public class VendorBankAccountService : IVendorBankAccountService
    {
        private readonly LiveSoldDbContext _context;
        private readonly IMapper _mapper;

        public VendorBankAccountService(LiveSoldDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<VendorBankAccountDto>> GetVendorBankAccountsAsync(Guid organizationId, Guid vendorId)
        {
            var accounts = await _context.VendorBankAccounts
                .Include(vba => vba.Vendor)
                .Where(vba => vba.Vendor.OrganizationId == organizationId && vba.VendorId == vendorId && vba.IsActive)
                .OrderBy(vba => vba.BankName)
                .ThenBy(vba => vba.AccountNumber)
                .ToListAsync();

            return _mapper.Map<List<VendorBankAccountDto>>(accounts);
        }

        public async Task<VendorBankAccountDto?> GetVendorBankAccountByIdAsync(Guid accountId, Guid organizationId)
        {
            var account = await _context.VendorBankAccounts
                .Include(vba => vba.Vendor)
                .FirstOrDefaultAsync(vba => vba.Id == accountId && vba.Vendor.OrganizationId == organizationId);

            return _mapper.Map<VendorBankAccountDto>(account);
        }

        public async Task<VendorBankAccountDto> CreateVendorBankAccountAsync(
            Guid organizationId,
            CreateVendorBankAccountDto dto)
        {
            // Validar que el proveedor existe
            var vendor = await _context.Vendors
                .FirstOrDefaultAsync(v => v.Id == dto.VendorId && v.OrganizationId == organizationId);

            if (vendor == null)
                throw new Exception($"Proveedor con ID {dto.VendorId} no encontrado");

            var account = new VendorBankAccount
            {
                Id = Guid.NewGuid(),
                VendorId = dto.VendorId,
                BankName = dto.BankName,
                AccountNumber = dto.AccountNumber,
                AccountHolderName = dto.AccountHolderName,
                CLABE_IBAN = dto.CLABE_IBAN,
                AccountType = dto.AccountType,
                IsPrimary = dto.IsPrimary,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.VendorBankAccounts.Add(account);
            await _context.SaveChangesAsync();

            return await GetVendorBankAccountByIdAsync(account.Id, organizationId)
                ?? throw new Exception("Error al recuperar la cuenta bancaria creada");
        }

        public async Task<VendorBankAccountDto> UpdateVendorBankAccountAsync(
            Guid accountId,
            Guid organizationId,
            UpdateVendorBankAccountDto dto)
        {
            var account = await _context.VendorBankAccounts
                .Include(vba => vba.Vendor)
                .FirstOrDefaultAsync(vba => vba.Id == accountId && vba.Vendor.OrganizationId == organizationId);

            if (account == null)
                throw new Exception($"Cuenta bancaria con ID {accountId} no encontrada");

            if (dto.BankName != null)
                account.BankName = dto.BankName;
            if (dto.AccountNumber != null)
                account.AccountNumber = dto.AccountNumber;
            if (dto.AccountHolderName != null)
                account.AccountHolderName = dto.AccountHolderName;
            if (dto.CLABE_IBAN != null)
                account.CLABE_IBAN = dto.CLABE_IBAN;
            if (dto.AccountType != null)
                account.AccountType = dto.AccountType;
            if (dto.IsPrimary.HasValue)
                account.IsPrimary = dto.IsPrimary.Value;
            if (dto.IsActive.HasValue)
                account.IsActive = dto.IsActive.Value;

            account.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetVendorBankAccountByIdAsync(account.Id, organizationId)
                ?? throw new Exception("Error al recuperar la cuenta bancaria actualizada");
        }

        public async Task DeleteVendorBankAccountAsync(Guid accountId, Guid organizationId)
        {
            var account = await _context.VendorBankAccounts
                .Include(vba => vba.Vendor)
                .FirstOrDefaultAsync(vba => vba.Id == accountId && vba.Vendor.OrganizationId == organizationId);

            if (account == null)
                throw new Exception($"Cuenta bancaria con ID {accountId} no encontrada");

            // Soft delete
            account.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
}
