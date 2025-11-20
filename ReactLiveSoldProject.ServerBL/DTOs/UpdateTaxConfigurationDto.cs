using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Base;

namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class UpdateTaxConfigurationDto
    {
        [Required]
        public bool TaxEnabled { get; set; }

        [Required]
        public TaxSystemType TaxSystemType { get; set; }

        [MaxLength(50)]
        public string? TaxDisplayName { get; set; }

        [Required]
        public TaxApplicationMode TaxApplicationMode { get; set; }

        public Guid? DefaultTaxRateId { get; set; }
    }
}
