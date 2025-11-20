using ReactLiveSoldProject.ServerBL.Base;

namespace ReactLiveSoldProject.ServerBL.DTOs
{
    /// <summary>
    /// DTO para la configuración de impuestos de una organización
    /// </summary>
    public class TaxConfigurationDto
    {
        public bool TaxEnabled { get; set; }
        public TaxSystemType TaxSystemType { get; set; }
        public string? TaxDisplayName { get; set; }
        public TaxApplicationMode TaxApplicationMode { get; set; }
        public Guid? DefaultTaxRateId { get; set; }
        public List<TaxRateDto> TaxRates { get; set; } = new List<TaxRateDto>();
    }
}
