using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Models.Configuration;

namespace ReactLiveSoldProject.ServerBL.DTOs.Configuration
{
    public class UpdateNoSerieDto
    {
        [StringLength(1000)]
        public string? Description { get; set; }

        public DocumentType? DocumentType { get; set; }

        public bool? DefaultNos { get; set; }

        public bool? ManualNos { get; set; }

        public bool? DateOrder { get; set; }
    }
}
