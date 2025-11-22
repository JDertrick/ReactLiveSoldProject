using System.ComponentModel.DataAnnotations;

namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class UpdateVendorDto
    {
        public Guid? ContactId { get; set; }

        public Guid? AssignedBuyerId { get; set; }

        [MaxLength(50)]
        public string? VendorCode { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(50)]
        public string? PaymentTerms { get; set; }

        public decimal? CreditLimit { get; set; }

        public bool? IsActive { get; set; }
    }
}
