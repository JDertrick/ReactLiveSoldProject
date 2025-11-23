using ReactLiveSoldProject.ServerBL.Base;
using System.ComponentModel.DataAnnotations;

namespace ReactLiveSoldProject.ServerBL.DTOs.Accounting
{
    public class CreateChartOfAccountDto
    {
        [Required]
        [StringLength(20)]
        public string AccountCode { get; set; }

        [Required]
        [StringLength(255)]
        public string AccountName { get; set; }

        [Required]
        public AccountType AccountType { get; set; }

        public string Description { get; set; }
    }
}
