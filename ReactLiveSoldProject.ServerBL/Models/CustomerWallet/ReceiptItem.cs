using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReactLiveSoldProject.ServerBL.Models.CustomerWallet
{
    public class ReceiptItem
    {
        public Guid Id { get; set; }

        [Required]
        public Guid ReceiptId { get; set; }
        public virtual Receipt Receipt { get; set; }

        [Required]
        [MaxLength(200)]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Subtotal { get; set; } // Should be UnitPrice * Quantity
    }
}
