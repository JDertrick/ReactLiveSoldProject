using System.ComponentModel.DataAnnotations;

namespace ReactLiveSoldProject.ServerBL.DTOs.Accounting
{
    public class CreateJournalEntryLineDto
    {
        [Required]
        public Guid AccountId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Debit { get; set; } = 0.00m;

        [Range(0, double.MaxValue)]
        public decimal Credit { get; set; } = 0.00m;
    }
}
