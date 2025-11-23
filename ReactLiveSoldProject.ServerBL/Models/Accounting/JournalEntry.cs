using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReactLiveSoldProject.ServerBL.Models.Accounting
{
    public class JournalEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid OrganizationId { get; set; }

        [Required]
        public DateTime EntryDate { get; set; }

        [Required]
        [StringLength(255)]
        public string Description { get; set; }

        public string ReferenceNumber { get; set; } // e.g., Invoice number, transaction ID

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property for JournalEntryLines
        public ICollection<JournalEntryLine> JournalEntryLines { get; set; }
    }
}
