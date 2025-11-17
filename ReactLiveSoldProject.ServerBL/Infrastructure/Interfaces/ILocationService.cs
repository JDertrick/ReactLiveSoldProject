
using ReactLiveSoldProject.ServerBL.DTOs;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface ILocationService
    {
        Task<List<LocationDto>> GetLocationsAsync(Guid organizationId);
        Task<LocationDto> GetLocationAsync(Guid organizationId, Guid id);
        Task CreateLocationAsync(Guid organizationId, CreateLocationDto dto);
        Task UpdateLocationAsync(Guid organizationId, Guid id, UpdateLocationDto dto);
        Task DeleteLocationAsync(Guid organizationId, Guid id);
    }
}
