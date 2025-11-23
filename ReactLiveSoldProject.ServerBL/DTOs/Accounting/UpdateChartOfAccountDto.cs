using ReactLiveSoldProject.ServerBL.Base;
using System.ComponentModel.DataAnnotations;

namespace ReactLiveSoldProject.ServerBL.DTOs.Accounting
{
    public class UpdateChartOfAccountDto
    {
        [StringLength(255)]
        public string AccountName { get; set; }

        public AccountType? AccountType { get; set; }

        public string Description { get; set; }

        public bool? IsActive { get; set; }
    }
}
