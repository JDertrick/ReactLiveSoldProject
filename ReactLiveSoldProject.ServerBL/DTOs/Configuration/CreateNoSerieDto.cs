using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Models.Configuration;

namespace ReactLiveSoldProject.ServerBL.DTOs.Configuration
{
    public class CreateNoSerieDto
    {
        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de documento (Customer, Vendor, Invoice, etc.)
        /// </summary>
        public DocumentType? DocumentType { get; set; }

        public bool DefaultNos { get; set; } = false;

        public bool ManualNos { get; set; } = false;

        public bool DateOrder { get; set; } = false;

        public List<CreateNoSerieLineDto>? NoSerieLines { get; set; }
    }
}