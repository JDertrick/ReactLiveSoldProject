using System.ComponentModel.DataAnnotations;

namespace ReactLiveSoldProject.ServerBL.DTOs.Configuration
{
    public class CreateNoSerieDto
    {
        [StringLength(20)]
        public string Code { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public bool DefaultNos { get; set; }

        public bool ManualNos { get; set; }

        public bool DateOrder { get; set; }

        public ICollection<CreateNoSerieLineDto> NoSerieLines { get; set; }
    }
}