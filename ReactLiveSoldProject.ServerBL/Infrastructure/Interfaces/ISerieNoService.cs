using ReactLiveSoldProject.ServerBL.DTOs.Configuration;
using ReactLiveSoldProject.ServerBL.Models.Configuration;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface ISerieNoService
    {
        // Operaciones CRUD de NoSerie
        Task<List<NoSerieDto>> GetAllAsync(Guid organizationId);
        Task<NoSerieDto?> GetByIdAsync(Guid organizationId, Guid id);
        Task<NoSerieDto?> GetByCodeAsync(Guid organizationId, string code);
        Task<NoSerieDto> CreateAsync(Guid organizationId, CreateNoSerieDto dto);
        Task<NoSerieDto> UpdateAsync(Guid organizationId, Guid id, UpdateNoSerieDto dto);
        Task DeleteAsync(Guid organizationId, Guid id);

        // Operaciones de NoSerieLine
        Task<NoSerieLineDto> AddLineAsync(Guid organizationId, Guid noSerieId, CreateNoSerieLineDto dto);
        Task<NoSerieLineDto> UpdateLineAsync(Guid organizationId, Guid lineId, UpdateNoSerieLineDto dto);
        Task DeleteLineAsync(Guid organizationId, Guid lineId);

        // Operaciones de numeración secuencial (la funcionalidad principal)
        Task<string> GetNextNumberAsync(Guid organizationId, string serieCode, DateTime? date = null);
        Task<string> GetNextNumberByTypeAsync(Guid organizationId, DocumentType documentType, DateTime? date = null);
        Task<List<NoSerieDto>> GetByDocumentTypeAsync(Guid organizationId, DocumentType documentType);
        Task<NoSerieDto?> GetDefaultSerieByTypeAsync(Guid organizationId, DocumentType documentType);

        // Validación
        Task<bool> ValidateNumberAsync(Guid organizationId, string serieCode, string number);
        Task<bool> IsNumberAvailableAsync(Guid organizationId, string serieCode, string number);
    }
}
