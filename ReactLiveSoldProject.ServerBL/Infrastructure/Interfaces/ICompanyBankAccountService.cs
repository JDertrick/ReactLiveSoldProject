using ReactLiveSoldProject.ServerBL.DTOs.Banking;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface ICompanyBankAccountService
    {
        Task<List<CompanyBankAccountDto>> GetCompanyBankAccountsAsync(Guid organizationId);
        Task<CompanyBankAccountDto?> GetCompanyBankAccountByIdAsync(Guid accountId, Guid organizationId);
        Task<CompanyBankAccountDto> CreateCompanyBankAccountAsync(Guid organizationId, CreateCompanyBankAccountDto dto);
        Task<CompanyBankAccountDto> UpdateCompanyBankAccountAsync(Guid accountId, Guid organizationId, UpdateCompanyBankAccountDto dto);
        Task DeleteCompanyBankAccountAsync(Guid accountId, Guid organizationId);
    }
}
