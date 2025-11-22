using System.ComponentModel.DataAnnotations;

namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class CreateVendorDto
    {
        [Required]
        public Guid ContactId { get; set; }

        public Guid? AssignedBuyerId { get; set; }

        [MaxLength(50)]
        public string? VendorCode { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(50)]
        public string? PaymentTerms { get; set; }

        public decimal CreditLimit { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}
