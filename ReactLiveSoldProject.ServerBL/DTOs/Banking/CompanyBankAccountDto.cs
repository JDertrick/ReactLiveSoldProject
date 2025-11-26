namespace ReactLiveSoldProject.ServerBL.DTOs.Banking
{
    public class CompanyBankAccountDto
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string Currency { get; set; }
        public decimal CurrentBalance { get; set; }
        public Guid GLAccountId { get; set; }
        public string? GLAccountName { get; set; } // Para display
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateCompanyBankAccountDto
    {
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string Currency { get; set; } = "MXN";
        public decimal CurrentBalance { get; set; } = 0;
        public Guid GLAccountId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateCompanyBankAccountDto
    {
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? Currency { get; set; }
        public decimal? CurrentBalance { get; set; }
        public Guid? GLAccountId { get; set; }
        public bool? IsActive { get; set; }
    }
}
