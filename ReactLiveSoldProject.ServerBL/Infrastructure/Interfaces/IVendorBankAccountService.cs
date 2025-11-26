using ReactLiveSoldProject.ServerBL.DTOs.Vendors;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface IVendorBankAccountService
    {
        Task<List<VendorBankAccountDto>> GetVendorBankAccountsAsync(Guid organizationId, Guid vendorId);
        Task<VendorBankAccountDto?> GetVendorBankAccountByIdAsync(Guid accountId, Guid organizationId);
        Task<VendorBankAccountDto> CreateVendorBankAccountAsync(Guid organizationId, CreateVendorBankAccountDto dto);
        Task<VendorBankAccountDto> UpdateVendorBankAccountAsync(Guid accountId, Guid organizationId, UpdateVendorBankAccountDto dto);
        Task DeleteVendorBankAccountAsync(Guid accountId, Guid organizationId);
    }
}
