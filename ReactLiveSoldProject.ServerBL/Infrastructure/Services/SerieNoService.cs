using Mapster;
using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs.Configuration;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Configuration;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    public class SerieNoService : ISerieNoService
    {
        private readonly LiveSoldDbContext _dbContext;

        public SerieNoService(LiveSoldDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region NoSerie CRUD Operations

        public async Task<List<NoSerieDto>> GetAllAsync(Guid organizationId)
        {
            try
            {
                var series = await _dbContext.NoSeries
                .Where(n => n.OrganizationId == organizationId)
                .Include(n => n.NoSerieLines)
                .OrderBy(n => n.Code)
                .ToListAsync();

                var seriest = series.Adapt<List<NoSerieDto>>();
                return seriest;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<NoSerieDto?> GetByIdAsync(Guid organizationId, Guid id)
        {
            var serie = await _dbContext.NoSeries
                .Include(n => n.NoSerieLines)
                .FirstOrDefaultAsync(n => n.OrganizationId == organizationId && n.Id == id);

            return serie != null ? MapToDto(serie) : null;
        }

        public async Task<NoSerieDto?> GetByCodeAsync(Guid organizationId, string code)
        {
            var serie = await _dbContext.NoSeries
                .Include(n => n.NoSerieLines)
                .FirstOrDefaultAsync(n => n.OrganizationId == organizationId && n.Code == code);

            return serie != null ? MapToDto(serie) : null;
        }

        public async Task<NoSerieDto> CreateAsync(Guid organizationId, CreateNoSerieDto dto)
        {
            // Verificar que no exista una serie con el mismo código
            var existingSerie = await _dbContext.NoSeries
                .FirstOrDefaultAsync(n => n.OrganizationId == organizationId && n.Code == dto.Code);

            if (existingSerie != null)
                throw new InvalidOperationException($"Ya existe una serie con el código '{dto.Code}'");

            // Si se marca como DefaultNos, desmarcar las demás series del mismo tipo
            if (dto.DefaultNos)
            {
                var existingDefaults = await _dbContext.NoSeries
                    .Where(n => n.OrganizationId == organizationId && n.DocumentType == dto.DocumentType && n.DefaultNos)
                    .ToListAsync();

                foreach (var existingDefault in existingDefaults)
                {
                    existingDefault.DefaultNos = false;
                }
            }
            // Si no se marca como DefaultNos, verificar si es la primera serie para este tipo
            else
            {
                var existingSeriesForType = await _dbContext.NoSeries
                    .Where(n => n.OrganizationId == organizationId && n.DocumentType == dto.DocumentType)
                    .CountAsync();

                // Si es la primera serie para este tipo, forzar DefaultNos = true
                if (existingSeriesForType == 0)
                {
                    dto.DefaultNos = true;
                }
            }

            var serie = new NoSerie
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                Code = dto.Code,
                Description = dto.Description,
                DocumentType = dto.DocumentType, // Ahora es requerido
                DefaultNos = dto.DefaultNos,
                ManualNos = dto.ManualNos,
                DateOrder = dto.DateOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.NoSeries.Add(serie);

            // Agregar las líneas si vienen
            if (dto.NoSerieLines != null && dto.NoSerieLines.Any())
            {
                foreach (var lineDto in dto.NoSerieLines)
                {
                    var line = new NoSerieLine
                    {
                        Id = Guid.NewGuid(),
                        NoSerieId = serie.Id,
                        StartingDate = lineDto.StartingDate,
                        StartingNo = lineDto.StartingNo,
                        EndingNo = lineDto.EndingNo,
                        LastNoUsed = string.Empty,
                        IncrementBy = lineDto.IncrementBy,
                        WarningNo = lineDto.WarningNo ?? string.Empty,
                        Open = lineDto.Open
                    };

                    _dbContext.NoSerieLines.Add(line);
                }
            }

            await _dbContext.SaveChangesAsync();

            // Recargar para incluir las líneas
            var createdSerie = await _dbContext.NoSeries
                .Include(n => n.NoSerieLines)
                .FirstAsync(n => n.Id == serie.Id);

            return MapToDto(createdSerie);
        }

        public async Task<NoSerieDto> UpdateAsync(Guid organizationId, Guid id, UpdateNoSerieDto dto)
        {
            var serie = await _dbContext.NoSeries
                .Include(n => n.NoSerieLines)
                .FirstOrDefaultAsync(n => n.OrganizationId == organizationId && n.Id == id);

            if (serie == null)
                throw new KeyNotFoundException("Serie numérica no encontrada");

            // Validar cambio de DocumentType
            if (dto.DocumentType.HasValue && dto.DocumentType != serie.DocumentType)
            {
                // Verificar si ya se han usado números de esta serie
                var hasUsedNumbers = serie.NoSerieLines.Any(l => !string.IsNullOrEmpty(l.LastNoUsed));
                if (hasUsedNumbers)
                {
                    throw new InvalidOperationException("No se puede cambiar el tipo de documento de una serie que ya ha generado números. Cree una nueva serie en su lugar.");
                }
            }

            // Si se intenta desmarcar DefaultNos, verificar que no sea la única serie activa para ese tipo
            if (dto.DefaultNos.HasValue && !dto.DefaultNos.Value && serie.DefaultNos)
            {
                var docType = dto.DocumentType ?? serie.DocumentType;
                if (docType.HasValue)
                {
                    var otherActiveSeriesCount = await _dbContext.NoSeries
                        .Where(n => n.OrganizationId == organizationId && n.DocumentType == docType && n.Id != id)
                        .CountAsync();

                    if (otherActiveSeriesCount == 0)
                    {
                        throw new InvalidOperationException($"No se puede desmarcar como serie por defecto porque es la única serie activa para el tipo '{docType}'. Debe existir al menos una serie por defecto para cada tipo de documento.");
                    }

                    // Si hay otras series, asegurarse de que al menos una esté marcada como DefaultNos
                    var hasOtherDefault = await _dbContext.NoSeries
                        .AnyAsync(n => n.OrganizationId == organizationId && n.DocumentType == docType && n.DefaultNos && n.Id != id);

                    if (!hasOtherDefault)
                    {
                        throw new InvalidOperationException($"No se puede desmarcar como serie por defecto porque no hay otra serie marcada como predeterminada para el tipo '{docType}'. Primero marque otra serie como predeterminada.");
                    }
                }
            }

            // Si se marca como DefaultNos y tiene un DocumentType, desmarcar las demás series del mismo tipo
            if (dto.DefaultNos.HasValue && dto.DefaultNos.Value)
            {
                var docType = dto.DocumentType ?? serie.DocumentType;
                if (docType.HasValue)
                {
                    var existingDefaults = await _dbContext.NoSeries
                        .Where(n => n.OrganizationId == organizationId && n.DocumentType == docType && n.DefaultNos && n.Id != id)
                        .ToListAsync();

                    foreach (var existingDefault in existingDefaults)
                    {
                        existingDefault.DefaultNos = false;
                    }
                }
            }

            // Actualizar campos
            if (dto.Description != null)
                serie.Description = dto.Description;

            if (dto.DocumentType.HasValue)
                serie.DocumentType = dto.DocumentType;

            if (dto.DefaultNos.HasValue)
                serie.DefaultNos = dto.DefaultNos.Value;

            if (dto.ManualNos.HasValue)
                serie.ManualNos = dto.ManualNos.Value;

            if (dto.DateOrder.HasValue)
                serie.DateOrder = dto.DateOrder.Value;

            serie.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return MapToDto(serie);
        }

        public async Task DeleteAsync(Guid organizationId, Guid id)
        {
            var serie = await _dbContext.NoSeries
                .Include(n => n.NoSerieLines)
                .FirstOrDefaultAsync(n => n.OrganizationId == organizationId && n.Id == id);

            if (serie == null)
                throw new KeyNotFoundException("Serie numérica no encontrada");

            // Verificar si ya se han usado números de esta serie
            var hasUsedNumbers = serie.NoSerieLines.Any(l => !string.IsNullOrEmpty(l.LastNoUsed));
            if (hasUsedNumbers)
            {
                throw new InvalidOperationException("No se puede eliminar una serie que ya ha generado números. Esta serie debe mantenerse para mantener la trazabilidad de los documentos.");
            }

            // Si es la serie por defecto, verificar que no sea la única para su tipo de documento
            if (serie.DefaultNos && serie.DocumentType.HasValue)
            {
                var otherSeriesCount = await _dbContext.NoSeries
                    .Where(n => n.OrganizationId == organizationId && n.DocumentType == serie.DocumentType && n.Id != id)
                    .CountAsync();

                if (otherSeriesCount == 0)
                {
                    throw new InvalidOperationException($"No se puede eliminar la única serie configurada para el tipo de documento '{serie.DocumentType}'. Debe existir al menos una serie para cada tipo de documento.");
                }
            }

            // Eliminar las líneas primero
            _dbContext.NoSerieLines.RemoveRange(serie.NoSerieLines);

            // Eliminar la serie
            _dbContext.NoSeries.Remove(serie);

            await _dbContext.SaveChangesAsync();
        }

        #endregion

        #region NoSerieLine Operations

        public async Task<NoSerieLineDto> AddLineAsync(Guid organizationId, Guid noSerieId, CreateNoSerieLineDto dto)
        {
            var serie = await _dbContext.NoSeries
                .FirstOrDefaultAsync(n => n.OrganizationId == organizationId && n.Id == noSerieId);

            if (serie == null)
                throw new KeyNotFoundException("Serie numérica no encontrada");

            // Validar que no haya solapamiento de fechas
            var overlappingLine = await _dbContext.NoSerieLines
                .Where(l => l.NoSerieId == noSerieId && l.Open && l.StartingDate == dto.StartingDate)
                .FirstOrDefaultAsync();

            if (overlappingLine != null)
                throw new InvalidOperationException($"Ya existe una línea abierta con la fecha {dto.StartingDate:yyyy-MM-dd}");

            var line = new NoSerieLine
            {
                Id = Guid.NewGuid(),
                NoSerieId = noSerieId,
                StartingDate = dto.StartingDate,
                StartingNo = dto.StartingNo,
                EndingNo = dto.EndingNo,
                LastNoUsed = string.Empty,
                IncrementBy = dto.IncrementBy,
                WarningNo = dto.WarningNo ?? string.Empty,
                Open = dto.Open
            };

            _dbContext.NoSerieLines.Add(line);
            await _dbContext.SaveChangesAsync();

            return line.Adapt<NoSerieLineDto>();
        }

        public async Task<NoSerieLineDto> UpdateLineAsync(Guid organizationId, Guid lineId, UpdateNoSerieLineDto dto)
        {
            var line = await _dbContext.NoSerieLines
                .Include(l => l.NoSerie)
                .FirstOrDefaultAsync(l => l.Id == lineId && l.NoSerie.OrganizationId == organizationId);

            if (line == null)
                throw new KeyNotFoundException("Línea de serie no encontrada");

            // Actualizar campos
            if (dto.StartingDate.HasValue)
                line.StartingDate = dto.StartingDate.Value;

            if (dto.StartingNo != null)
                line.StartingNo = dto.StartingNo;

            if (dto.EndingNo != null)
                line.EndingNo = dto.EndingNo;

            if (dto.IncrementBy.HasValue)
                line.IncrementBy = dto.IncrementBy.Value;

            if (dto.WarningNo != null)
                line.WarningNo = dto.WarningNo;

            if (dto.Open.HasValue)
                line.Open = dto.Open.Value;

            await _dbContext.SaveChangesAsync();

            return line.Adapt<NoSerieLineDto>();
        }

        public async Task DeleteLineAsync(Guid organizationId, Guid lineId)
        {
            var line = await _dbContext.NoSerieLines
                .Include(l => l.NoSerie)
                .FirstOrDefaultAsync(l => l.Id == lineId && l.NoSerie.OrganizationId == organizationId);

            if (line == null)
                throw new KeyNotFoundException("Línea de serie no encontrada");

            _dbContext.NoSerieLines.Remove(line);
            await _dbContext.SaveChangesAsync();
        }

        #endregion

        #region Number Generation (Core Functionality)

        public async Task<string> GetNextNumberAsync(Guid organizationId, string serieCode, DateTime? date = null)
        {
            var serie = await _dbContext.NoSeries
                .Include(n => n.NoSerieLines)
                .FirstOrDefaultAsync(n => n.OrganizationId == organizationId && n.Code == serieCode);

            if (serie == null)
                throw new KeyNotFoundException($"Serie numérica '{serieCode}' no encontrada");

            return await GetNextNumberFromSerieAsync(serie, date ?? DateTime.UtcNow);
        }

        public async Task<string> GetNextNumberByTypeAsync(Guid organizationId, DocumentType documentType, DateTime? date = null)
        {
            // Buscar la serie por defecto para este tipo de documento
            var serie = await _dbContext.NoSeries
                .Include(n => n.NoSerieLines)
                .FirstOrDefaultAsync(n => n.OrganizationId == organizationId && n.DocumentType == documentType && n.DefaultNos);

            if (serie == null)
                throw new InvalidOperationException($"No se puede crear el documento: No hay una serie numérica configurada como predeterminada para el tipo '{documentType}'. Por favor, configure una serie numérica en la sección de Configuración antes de crear este tipo de documento.");

            return await GetNextNumberFromSerieAsync(serie, date ?? DateTime.UtcNow);
        }

        public async Task<List<NoSerieDto>> GetByDocumentTypeAsync(Guid organizationId, DocumentType documentType)
        {
            var series = await _dbContext.NoSeries
                .Where(n => n.OrganizationId == organizationId && n.DocumentType == documentType)
                .Include(n => n.NoSerieLines)
                .OrderBy(n => n.Code)
                .ToListAsync();

            return series.Select(MapToDto).ToList();
        }

        public async Task<NoSerieDto?> GetDefaultSerieByTypeAsync(Guid organizationId, DocumentType documentType)
        {
            var serie = await _dbContext.NoSeries
                .Include(n => n.NoSerieLines)
                .FirstOrDefaultAsync(n => n.OrganizationId == organizationId && n.DocumentType == documentType && n.DefaultNos);

            return serie != null ? MapToDto(serie) : null;
        }

        private async Task<string> GetNextNumberFromSerieAsync(NoSerie serie, DateTime date)
        {
            // Buscar la línea apropiada para esta fecha
            var line = serie.NoSerieLines
                .Where(l => l.Open && l.StartingDate <= date)
                .OrderByDescending(l => l.StartingDate)
                .FirstOrDefault();

            if (line == null)
                throw new InvalidOperationException($"No hay ninguna línea abierta disponible para la fecha {date:yyyy-MM-dd} en la serie '{serie.Code}'");

            // Determinar el siguiente número
            string nextNumber;
            if (string.IsNullOrEmpty(line.LastNoUsed))
            {
                // Primera vez, usar el StartingNo
                nextNumber = line.StartingNo;
            }
            else
            {
                // Incrementar desde el último número usado
                nextNumber = IncrementNumber(line.LastNoUsed, line.IncrementBy);
            }

            // Validar que no exceda el EndingNo
            if (CompareNumbers(nextNumber, line.EndingNo) > 0)
            {
                throw new InvalidOperationException($"Se ha alcanzado el número final ({line.EndingNo}) de la serie '{serie.Code}'. Por favor, cree una nueva línea o ajuste el rango.");
            }

            // Verificar si estamos cerca del WarningNo
            if (!string.IsNullOrEmpty(line.WarningNo) && CompareNumbers(nextNumber, line.WarningNo) >= 0)
            {
                // Podríamos loguear o notificar aquí
                // Por ahora solo continúa
            }

            // Actualizar LastNoUsed
            line.LastNoUsed = nextNumber;
            await _dbContext.SaveChangesAsync();

            return nextNumber;
        }

        #endregion

        #region Validation

        public async Task<bool> ValidateNumberAsync(Guid organizationId, string serieCode, string number)
        {
            var serie = await _dbContext.NoSeries
                .Include(n => n.NoSerieLines)
                .FirstOrDefaultAsync(n => n.OrganizationId == organizationId && n.Code == serieCode);

            if (serie == null)
                return false;

            // Verificar si el número está dentro de algún rango válido
            foreach (var line in serie.NoSerieLines.Where(l => l.Open))
            {
                if (CompareNumbers(number, line.StartingNo) >= 0 && CompareNumbers(number, line.EndingNo) <= 0)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> IsNumberAvailableAsync(Guid organizationId, string serieCode, string number)
        {
            var serie = await _dbContext.NoSeries
                .Include(n => n.NoSerieLines)
                .FirstOrDefaultAsync(n => n.OrganizationId == organizationId && n.Code == serieCode);

            if (serie == null)
                return false;

            foreach (var line in serie.NoSerieLines.Where(l => l.Open))
            {
                if (CompareNumbers(number, line.StartingNo) >= 0 && CompareNumbers(number, line.EndingNo) <= 0)
                {
                    // El número está en el rango, verificar si ya fue usado
                    if (string.IsNullOrEmpty(line.LastNoUsed))
                    {
                        // No se ha usado ningún número todavía
                        return CompareNumbers(number, line.StartingNo) >= 0;
                    }
                    else
                    {
                        // Verificar que sea mayor al último usado
                        return CompareNumbers(number, line.LastNoUsed) > 0;
                    }
                }
            }

            return false;
        }

        #endregion

        #region Helper Methods

        private static NoSerieDto MapToDto(NoSerie serie)
        {
            var dto = serie.Adapt<NoSerieDto>();
            dto.DocumentTypeName = serie.DocumentType?.ToString();
            return dto;
        }

        /// <summary>
        /// Incrementa un número alfanumérico (ej: "INV-0001" -> "INV-0002")
        /// </summary>
        private static string IncrementNumber(string number, int incrementBy)
        {
            // Separar prefijo (letras y caracteres especiales) del número
            int numStartIndex = number.Length - 1;
            while (numStartIndex >= 0 && char.IsDigit(number[numStartIndex]))
            {
                numStartIndex--;
            }
            numStartIndex++;

            if (numStartIndex >= number.Length)
            {
                // No hay parte numérica, no se puede incrementar
                throw new InvalidOperationException($"El número '{number}' no contiene una parte numérica que se pueda incrementar");
            }

            string prefix = number.Substring(0, numStartIndex);
            string numericPart = number.Substring(numStartIndex);
            int length = numericPart.Length;

            // Convertir a entero, incrementar y volver a formatear
            if (!int.TryParse(numericPart, out int numericValue))
            {
                throw new InvalidOperationException($"No se pudo parsear la parte numérica de '{number}'");
            }

            numericValue += incrementBy;

            // Formatear con ceros a la izquierda para mantener la longitud
            string newNumericPart = numericValue.ToString().PadLeft(length, '0');

            return prefix + newNumericPart;
        }

        /// <summary>
        /// Compara dos números alfanuméricos
        /// Retorna: -1 si num1 < num2, 0 si son iguales, 1 si num1 > num2
        /// </summary>
        private static int CompareNumbers(string num1, string num2)
        {
            // Extraer la parte numérica de ambos
            int GetNumericPart(string number)
            {
                int numStartIndex = number.Length - 1;
                while (numStartIndex >= 0 && char.IsDigit(number[numStartIndex]))
                {
                    numStartIndex--;
                }
                numStartIndex++;

                if (numStartIndex >= number.Length)
                    return 0;

                string numericPart = number.Substring(numStartIndex);
                return int.TryParse(numericPart, out int result) ? result : 0;
            }

            int value1 = GetNumericPart(num1);
            int value2 = GetNumericPart(num2);

            return value1.CompareTo(value2);
        }

        #endregion
    }
}
