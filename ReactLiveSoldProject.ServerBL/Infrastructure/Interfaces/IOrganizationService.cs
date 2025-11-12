using ReactLiveSoldProject.ServerBL.DTOs;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface IOrganizationService
    {
        Task<List<OrganizationDto>> GetAllOrganizationsAsync();
        Task<OrganizationDto?> GetOrganizationByIdAsync(Guid id);
        Task<OrganizationPublicDto?> GetOrganizationBySlugAsync(string slug);
        Task<OrganizationDto> CreateOrganizationAsync(CreateOrganizationDto dto);
        Task<OrganizationDto> UpdateOrganizationAsync(Guid id, CreateOrganizationDto dto);
        Task DeleteOrganizationAsync(Guid id);
    }
}
