namespace ReactLiveSoldProject.ServerBL.DTOs.Vendors
{
    public class VendorBankAccountDto
    {
        public Guid Id { get; set; }
        public Guid VendorId { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string? AccountHolderName { get; set; }
        public string? CLABE_IBAN { get; set; }
        public string? AccountType { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateVendorBankAccountDto
    {
        public Guid VendorId { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string? AccountHolderName { get; set; }
        public string? CLABE_IBAN { get; set; }
        public string? AccountType { get; set; }
        public bool IsPrimary { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }

    public class UpdateVendorBankAccountDto
    {
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountHolderName { get; set; }
        public string? CLABE_IBAN { get; set; }
        public string? AccountType { get; set; }
        public bool? IsPrimary { get; set; }
        public bool? IsActive { get; set; }
    }
}
