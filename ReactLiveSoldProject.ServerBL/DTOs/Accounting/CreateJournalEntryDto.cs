using System.ComponentModel.DataAnnotations;

namespace ReactLiveSoldProject.ServerBL.DTOs.Accounting
{
    public class CreateJournalEntryDto
    {
        [Required]
        public DateTime EntryDate { get; set; }

        [Required]
        [StringLength(255)]
        public string Description { get; set; }

        [StringLength(100)]
        public string ReferenceNumber { get; set; }

        [Required]
        [MinLength(2)] // Must have at least one debit and one credit line
        public List<CreateJournalEntryLineDto> Lines { get; set; } = new List<CreateJournalEntryLineDto>();
    }
}
