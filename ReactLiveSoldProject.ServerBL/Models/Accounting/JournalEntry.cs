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

        [MaxLength(50)]
        public string EntryNumber { get; set; } // JE-2025-000001

        [Required]
        public DateTime EntryDate { get; set; }

        [Required]
        [StringLength(255)]
        public string Description { get; set; }

        public string? ReferenceNumber { get; set; } // e.g., Invoice number, transaction ID

        [MaxLength(50)]
        public string? DocumentType { get; set; } // "RECEIPT", "PAYMENT", "INVOICE", etc.

        [MaxLength(50)]
        public string? DocumentNumber { get; set; } // PO-001, PAY-001, etc.

        public Guid? VendorId { get; set; } // Para trazabilidad

        [MaxLength(20)]
        public string? Status { get; set; } = "Posted"; // Posted, Draft, Void

        public Guid? PostedBy { get; set; } // Usuario que creó el asiento

        public DateTime? PostedDate { get; set; }

        [MaxLength(3)]
        public string Currency { get; set; } = "MXN";

        [Column(TypeName = "decimal(18, 6)")]
        public decimal ExchangeRate { get; set; } = 1.0m;

        public Guid CreatedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for JournalEntryLines
        public ICollection<JournalEntryLine> JournalEntryLines { get; set; }
    }
}
