using System.ComponentModel.DataAnnotations;

namespace ReactLiveSoldProject.ServerBL.DTOs.Configuration
{
    public class NoSerieLineDto
    {
        public Guid Id { get; set; }

        public Guid NoSerieId { get; set; }

        public DateTime StartingDate { get; set; }

        [StringLength(20)]
        public string StartingNo { get; set; } = string.Empty;

        [StringLength(20)]
        public string EndingNo { get; set; } = string.Empty;

        [StringLength(20)]
        public string LastNoUsed { get; set; } = string.Empty;

        public int IncrementBy { get; set; } = 1;

        [StringLength(20)]
        public string WarningNo { get; set; } = string.Empty;

        public bool Open { get; set; } = true;
    }
}