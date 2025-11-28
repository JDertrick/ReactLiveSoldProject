using ReactLiveSoldProject.ServerBL.DTOs.Accounting;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface IOrganizationAccountConfigurationService
    {
        Task<OrganizationAccountConfigurationDto?> GetConfigurationAsync(Guid organizationId);
        Task<OrganizationAccountConfigurationDto> CreateOrUpdateConfigurationAsync(Guid organizationId, UpdateOrganizationAccountConfigurationDto dto);
    }
}
