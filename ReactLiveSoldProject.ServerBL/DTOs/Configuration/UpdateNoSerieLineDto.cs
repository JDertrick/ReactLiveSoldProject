using System.ComponentModel.DataAnnotations;

namespace ReactLiveSoldProject.ServerBL.DTOs.Configuration
{
    public class UpdateNoSerieLineDto
    {
        public DateTime? StartingDate { get; set; }

        [StringLength(20)]
        public string? StartingNo { get; set; }

        [StringLength(20)]
        public string? EndingNo { get; set; }

        public int? IncrementBy { get; set; }

        [StringLength(20)]
        public string? WarningNo { get; set; }

        public bool? Open { get; set; }
    }
}
