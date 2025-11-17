
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Inventory;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    public class LocationService : ILocationService
    {
        private readonly LiveSoldDbContext _dbContext;
        private readonly IMapper _mapper;

        public LocationService(LiveSoldDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<LocationDto>> GetLocationsAsync(Guid organizationId)
        {
            try
            {
                var locationsDto = await _dbContext.Locations
                    .Where(l => l.OrganizationId == organizationId)
                    .OrderByDescending(l => l.Name)
                    .ProjectTo<LocationDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return locationsDto;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<LocationDto> GetLocationAsync(Guid organizationId, Guid id)
        {
            try
            {
                var locationDto = await _dbContext.Locations
                    .Where(l => l.OrganizationId == organizationId && l.Id == id)
                    .ProjectTo<LocationDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();

                return locationDto ?? new();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task CreateLocationAsync(Guid organizationId, CreateLocationDto dto)
        {
            try
            {
                var location = _mapper.Map<Location>(dto);
                location.OrganizationId = organizationId;

                _dbContext.Locations.Add(location);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateLocationAsync(Guid organizationId, Guid id, UpdateLocationDto dto)
        {
            try
            {
                var location = await _dbContext.Locations.FindAsync(id);

                if (location == null || location.OrganizationId != organizationId)
                    throw new InvalidOperationException("No se puede actualizar una location que no existe o no pertenece a esta organización");

                _mapper.Map(dto, location);
                location.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task DeleteLocationAsync(Guid organizationId, Guid id)
        {
            try
            {
                var location = await _dbContext.Locations.FindAsync(id);

                if (location == null || location.OrganizationId != organizationId)
                    throw new InvalidOperationException("No se puede eliminar una location que no existe o no pertenece a esta organización");

                _dbContext.Locations.Remove(location);

                await _dbContext.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
