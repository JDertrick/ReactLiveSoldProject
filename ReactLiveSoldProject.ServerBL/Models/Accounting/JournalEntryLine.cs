using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReactLiveSoldProject.ServerBL.Models.Accounting
{
    public class JournalEntryLine
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid JournalEntryId { get; set; }

        [ForeignKey("JournalEntryId")]
        public JournalEntry JournalEntry { get; set; }

        public int LineNumber { get; set; }

        [Required]
        public Guid AccountId { get; set; }

        [ForeignKey("AccountId")]
        public ChartOfAccount Account { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal DebitAmount { get; set; } = 0.00m;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal CreditAmount { get; set; } = 0.00m;

        public Guid? VendorId { get; set; } // Para trazabilidad

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
