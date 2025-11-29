using System.ComponentModel.DataAnnotations;

namespace ReactLiveSoldProject.ServerBL.Models.Configuration
{
    public class NoSerie
    {
        public Guid Id { get; set; }

        public Guid OrganizationId { get; set; }

        [StringLength(20)]
        public string Code { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public bool DefaultNos { get; set; }

        public bool ManualNos { get; set; }

        public bool DateOrder { get; set; }

        public ICollection<NoSerieLine> NoSerieLines { get; set; }
    }
}