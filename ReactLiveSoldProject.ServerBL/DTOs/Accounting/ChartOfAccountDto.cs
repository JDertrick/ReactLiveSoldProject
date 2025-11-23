using ReactLiveSoldProject.ServerBL.Base;

namespace ReactLiveSoldProject.ServerBL.DTOs.Accounting
{
    public class ChartOfAccountDto
    {
        public Guid Id { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public AccountType AccountType { get; set; }
        public SystemAccountType? SystemAccountType { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}
