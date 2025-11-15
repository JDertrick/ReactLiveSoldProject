using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Base;

namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class CreateReceiptDto
    {
        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public TransactionType Type { get; set; } // Deposit or Withdrawal

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "El recibo debe tener al menos un ítem.")]
        public List<CreateReceiptItemDto> Items { get; set; } = new List<CreateReceiptItemDto>();
    }

    public class CreateReceiptItemDto
    {
        [Required]
        [MaxLength(200)]
        public string Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
