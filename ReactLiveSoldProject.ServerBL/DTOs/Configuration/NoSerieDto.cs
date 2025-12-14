using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Models.Configuration;

namespace ReactLiveSoldProject.ServerBL.DTOs.Configuration
{
    public class NoSerieDto
    {
        public Guid Id { get; set; }

        public Guid OrganizationId { get; set; }

        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public DocumentType? DocumentType { get; set; }

        public string? DocumentTypeName { get; set; }

        public bool DefaultNos { get; set; }

        public bool ManualNos { get; set; }

        public bool DateOrder { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public ICollection<NoSerieLineDto> NoSerieLines { get; set; } = new List<NoSerieLineDto>();
    }
}