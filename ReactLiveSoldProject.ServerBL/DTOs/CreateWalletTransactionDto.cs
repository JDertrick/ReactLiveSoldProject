using System.ComponentModel.DataAnnotations;

namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class CreateWalletTransactionDto
    {
        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string? Reference { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
