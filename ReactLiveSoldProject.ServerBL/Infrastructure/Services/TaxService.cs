using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Taxes;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    public class TaxService : ITaxService
    {
        private readonly LiveSoldDbContext _dbContext;

        public TaxService(LiveSoldDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ==================== CONFIGURACIÓN DE IMPUESTOS ====================

        public async Task<TaxConfigurationDto> GetTaxConfigurationAsync(Guid organizationId)
        {
            var org = await _dbContext.Organizations
                .Include(o => o.TaxRates.Where(tr => tr.IsActive))
                .FirstOrDefaultAsync(o => o.Id == organizationId);

            if (org == null)
                throw new KeyNotFoundException("Organización no encontrada");

            return new TaxConfigurationDto
            {
                TaxEnabled = org.TaxEnabled,
                TaxSystemType = org.TaxSystemType,
                TaxDisplayName = org.TaxDisplayName,
                TaxApplicationMode = org.TaxApplicationMode,
                DefaultTaxRateId = org.DefaultTaxRateId,
                TaxRates = org.TaxRates.Select(tr => new TaxRateDto
                {
                    Id = tr.Id,
                    OrganizationId = tr.OrganizationId,
                    Name = tr.Name,
                    Rate = tr.Rate,
                    IsDefault = tr.IsDefault,
                    IsActive = tr.IsActive,
                    Description = tr.Description,
                    EffectiveFrom = tr.EffectiveFrom,
                    EffectiveTo = tr.EffectiveTo,
                    CreatedAt = tr.CreatedAt,
                    UpdatedAt = tr.UpdatedAt
                }).ToList()
            };
        }

        public async Task UpdateTaxConfigurationAsync(Guid organizationId, UpdateTaxConfigurationDto dto)
        {
            var org = await _dbContext.Organizations.FindAsync(organizationId);
            if (org == null)
                throw new KeyNotFoundException("Organización no encontrada");

            org.TaxEnabled = dto.TaxEnabled;
            org.TaxSystemType = dto.TaxSystemType;
            org.TaxDisplayName = dto.TaxDisplayName;
            org.TaxApplicationMode = dto.TaxApplicationMode;
            org.DefaultTaxRateId = dto.DefaultTaxRateId;
            org.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        // ==================== GESTIÓN DE TASAS ====================

        public async Task<List<TaxRateDto>> GetTaxRatesAsync(Guid organizationId)
        {
            var rates = await _dbContext.Set<TaxRate>()
                .Where(tr => tr.OrganizationId == organizationId)
                .OrderByDescending(tr => tr.IsDefault)
                .ThenBy(tr => tr.Name)
                .ToListAsync();

            return rates.Select(tr => new TaxRateDto
            {
                Id = tr.Id,
                OrganizationId = tr.OrganizationId,
                Name = tr.Name,
                Rate = tr.Rate,
                IsDefault = tr.IsDefault,
                IsActive = tr.IsActive,
                Description = tr.Description,
                EffectiveFrom = tr.EffectiveFrom,
                EffectiveTo = tr.EffectiveTo,
                CreatedAt = tr.CreatedAt,
                UpdatedAt = tr.UpdatedAt
            }).ToList();
        }

        public async Task<TaxRateDto?> GetTaxRateByIdAsync(Guid taxRateId, Guid organizationId)
        {
            var rate = await _dbContext.Set<TaxRate>()
                .FirstOrDefaultAsync(tr => tr.Id == taxRateId && tr.OrganizationId == organizationId);

            if (rate == null)
                return null;

            return new TaxRateDto
            {
                Id = rate.Id,
                OrganizationId = rate.OrganizationId,
                Name = rate.Name,
                Rate = rate.Rate,
                IsDefault = rate.IsDefault,
                IsActive = rate.IsActive,
                Description = rate.Description,
                EffectiveFrom = rate.EffectiveFrom,
                EffectiveTo = rate.EffectiveTo,
                CreatedAt = rate.CreatedAt,
                UpdatedAt = rate.UpdatedAt
            };
        }

        public async Task<TaxRateDto?> GetDefaultTaxRateAsync(Guid organizationId)
        {
            var rate = await _dbContext.Set<TaxRate>()
                .FirstOrDefaultAsync(tr => tr.OrganizationId == organizationId && tr.IsDefault && tr.IsActive);

            if (rate == null)
                return null;

            return new TaxRateDto
            {
                Id = rate.Id,
                OrganizationId = rate.OrganizationId,
                Name = rate.Name,
                Rate = rate.Rate,
                IsDefault = rate.IsDefault,
                IsActive = rate.IsActive,
                Description = rate.Description,
                EffectiveFrom = rate.EffectiveFrom,
                EffectiveTo = rate.EffectiveTo,
                CreatedAt = rate.CreatedAt,
                UpdatedAt = rate.UpdatedAt
            };
        }

        public async Task<TaxRateDto> CreateTaxRateAsync(Guid organizationId, CreateTaxRateDto dto)
        {
            // Si se marca como default, desmarcar las demás
            if (dto.IsDefault)
            {
                var existingDefaults = await _dbContext.Set<TaxRate>()
                    .Where(tr => tr.OrganizationId == organizationId && tr.IsDefault)
                    .ToListAsync();

                foreach (var rate in existingDefaults)
                {
                    rate.IsDefault = false;
                }
            }

            var newRate = new TaxRate
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                Name = dto.Name,
                Rate = dto.Rate,
                IsDefault = dto.IsDefault,
                IsActive = dto.IsActive,
                Description = dto.Description,
                EffectiveFrom = dto.EffectiveFrom,
                EffectiveTo = dto.EffectiveTo,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Set<TaxRate>().Add(newRate);
            await _dbContext.SaveChangesAsync();

            return new TaxRateDto
            {
                Id = newRate.Id,
                OrganizationId = newRate.OrganizationId,
                Name = newRate.Name,
                Rate = newRate.Rate,
                IsDefault = newRate.IsDefault,
                IsActive = newRate.IsActive,
                Description = newRate.Description,
                EffectiveFrom = newRate.EffectiveFrom,
                EffectiveTo = newRate.EffectiveTo,
                CreatedAt = newRate.CreatedAt,
                UpdatedAt = newRate.UpdatedAt
            };
        }

        public async Task<TaxRateDto> UpdateTaxRateAsync(Guid organizationId, UpdateTaxRateDto dto)
        {
            var rate = await _dbContext.Set<TaxRate>()
                .FirstOrDefaultAsync(tr => tr.Id == dto.Id && tr.OrganizationId == organizationId);

            if (rate == null)
                throw new KeyNotFoundException("Tasa de impuesto no encontrada");

            // Si se marca como default, desmarcar las demás
            if (dto.IsDefault && !rate.IsDefault)
            {
                var existingDefaults = await _dbContext.Set<TaxRate>()
                    .Where(tr => tr.OrganizationId == organizationId && tr.IsDefault && tr.Id != dto.Id)
                    .ToListAsync();

                foreach (var existingRate in existingDefaults)
                {
                    existingRate.IsDefault = false;
                }
            }

            rate.Name = dto.Name;
            rate.Rate = dto.Rate;
            rate.IsDefault = dto.IsDefault;
            rate.IsActive = dto.IsActive;
            rate.Description = dto.Description;
            rate.EffectiveFrom = dto.EffectiveFrom;
            rate.EffectiveTo = dto.EffectiveTo;
            rate.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return new TaxRateDto
            {
                Id = rate.Id,
                OrganizationId = rate.OrganizationId,
                Name = rate.Name,
                Rate = rate.Rate,
                IsDefault = rate.IsDefault,
                IsActive = rate.IsActive,
                Description = rate.Description,
                EffectiveFrom = rate.EffectiveFrom,
                EffectiveTo = rate.EffectiveTo,
                CreatedAt = rate.CreatedAt,
                UpdatedAt = rate.UpdatedAt
            };
        }

        public async Task DeleteTaxRateAsync(Guid taxRateId, Guid organizationId)
        {
            var rate = await _dbContext.Set<TaxRate>()
                .FirstOrDefaultAsync(tr => tr.Id == taxRateId && tr.OrganizationId == organizationId);

            if (rate == null)
                throw new KeyNotFoundException("Tasa de impuesto no encontrada");

            // Verificar que no esté en uso
            var inUse = await _dbContext.SalesOrderItems
                .AnyAsync(soi => soi.TaxRateId == taxRateId);

            if (inUse)
            {
                throw new InvalidOperationException("No se puede eliminar una tasa de impuesto que está en uso");
            }

            _dbContext.Set<TaxRate>().Remove(rate);
            await _dbContext.SaveChangesAsync();
        }

        // ==================== CÁLCULOS DE IMPUESTOS ====================

        public async Task<TaxCalculationResult> CalculateTaxAsync(
            Guid organizationId,
            decimal amount,
            Guid? taxRateId = null,
            bool priceIncludesTax = true)
        {
            var org = await _dbContext.Organizations.FindAsync(organizationId);
            if (org == null)
                throw new KeyNotFoundException("Organización no encontrada");

            // Si los impuestos no están habilitados
            if (!org.TaxEnabled)
            {
                return new TaxCalculationResult
                {
                    Subtotal = amount,
                    TaxAmount = 0,
                    Total = amount,
                    TaxRate = 0,
                    TaxRateId = null
                };
            }

            // Obtener la tasa de impuesto
            TaxRate? rate = null;
            if (taxRateId.HasValue)
            {
                rate = await _dbContext.Set<TaxRate>()
                    .FirstOrDefaultAsync(tr => tr.Id == taxRateId.Value && tr.OrganizationId == organizationId && tr.IsActive);
            }
            else if (org.DefaultTaxRateId.HasValue)
            {
                rate = await _dbContext.Set<TaxRate>()
                    .FirstOrDefaultAsync(tr => tr.Id == org.DefaultTaxRateId.Value && tr.IsActive);
            }

            // Si no hay tasa, devolver sin impuesto
            if (rate == null)
            {
                return new TaxCalculationResult
                {
                    Subtotal = amount,
                    TaxAmount = 0,
                    Total = amount,
                    TaxRate = 0,
                    TaxRateId = null
                };
            }

            decimal subtotal, taxAmount, total;

            if (priceIncludesTax || org.TaxApplicationMode == TaxApplicationMode.TaxIncluded)
            {
                // Precio YA incluye impuesto (común en Europa/Latinoamérica)
                // Ejemplo: $119 incluye IVA 19%
                // Subtotal = $119 / 1.19 = $100
                // Impuesto = $100 * 0.19 = $19
                total = amount;
                subtotal = amount / (1 + rate.Rate);
                taxAmount = total - subtotal;
            }
            else
            {
                // Impuesto se AGREGA al precio (común en USA/Canadá)
                // Ejemplo: $100 + Sales Tax 8%
                // Subtotal = $100
                // Impuesto = $100 * 0.08 = $8
                // Total = $108
                subtotal = amount;
                taxAmount = amount * rate.Rate;
                total = subtotal + taxAmount;
            }

            return new TaxCalculationResult
            {
                Subtotal = Math.Round(subtotal, 2),
                TaxAmount = Math.Round(taxAmount, 2),
                Total = Math.Round(total, 2),
                TaxRate = rate.Rate,
                TaxRateId = rate.Id
            };
        }
    }
}
