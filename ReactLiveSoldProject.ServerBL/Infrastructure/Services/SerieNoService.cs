using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs.Configuration;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Configuration;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    public class SerieNoService : ISerieNoService
    {
        public readonly LiveSoldDbContext _dbContext;

        public SerieNoService(LiveSoldDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<NoSerieDto>> GetSerieNosAsync(Guid organizationId)
        {
            try
            {
                var seriesDtos = await _dbContext.NoSeries
                    .Where(n => n.OrganizationId == organizationId)
                    .Include(nl => nl.NoSerieLines)
                    .ProjectToType<NoSerieDto>().ToListAsync();

                return seriesDtos;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<NoSerieDto> GetSerieNoAsync(Guid organizationId, Guid id)
        {
            try
            {
                var serieDto = await _dbContext.NoSeries
                    .Where(n => n.OrganizationId == organizationId && n.Id == id)
                    .Include(nl => nl.NoSerieLines)
                    .ProjectToType<NoSerieDto>()
                    .FirstOrDefaultAsync();

                return serieDto ?? new();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task CreateSerieNoAsync(Guid organizationId, CreateNoSerieDto dto)
        {
            try
            {
                var noSerie = _dbContext.NoSeries
                    .Where(n => n.OrganizationId == organizationId && n.Code == dto.Code);

                if (noSerie.Any())
                    throw new InvalidOperationException("No se puede crear un numero serial con un codigo existente");

                var createNoSerie = dto.Adapt<NoSerie>();

                var c = await _dbContext.NoSeries.AddAsync(createNoSerie);
                createNoSerie = c.Entity;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
