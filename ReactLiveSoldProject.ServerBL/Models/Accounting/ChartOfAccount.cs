using ReactLiveSoldProject.ServerBL.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReactLiveSoldProject.ServerBL.Models.Accounting
{
    public class ChartOfAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [StringLength(20)]
        public string AccountCode { get; set; } // e.g., 1000, 2010, 5000

        [Required]
        [StringLength(255)]
        public string AccountName { get; set; } // e.g., Cash, Accounts Payable, Sales Revenue

        [Required]
        public AccountType AccountType { get; set; }

        public SystemAccountType? SystemAccountType { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public Guid OrganizationId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
