using ReactLiveSoldProject.ServerBL.DTOs;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface ITaxService
    {
        // Configuración de impuestos
        Task<TaxConfigurationDto> GetTaxConfigurationAsync(Guid organizationId);
        Task UpdateTaxConfigurationAsync(Guid organizationId, UpdateTaxConfigurationDto dto);

        // Gestión de tasas
        Task<List<TaxRateDto>> GetTaxRatesAsync(Guid organizationId);
        Task<TaxRateDto?> GetTaxRateByIdAsync(Guid taxRateId, Guid organizationId);
        Task<TaxRateDto?> GetDefaultTaxRateAsync(Guid organizationId);
        Task<TaxRateDto> CreateTaxRateAsync(Guid organizationId, CreateTaxRateDto dto);
        Task<TaxRateDto> UpdateTaxRateAsync(Guid organizationId, UpdateTaxRateDto dto);
        Task DeleteTaxRateAsync(Guid taxRateId, Guid organizationId);

        // Cálculos de impuestos
        Task<TaxCalculationResult> CalculateTaxAsync(Guid organizationId, decimal amount, Guid? taxRateId = null, bool priceIncludesTax = true);
    }

    public class TaxCalculationResult
    {
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public decimal TaxRate { get; set; }
        public Guid? TaxRateId { get; set; }
    }
}
