using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.Models.Authentication;

namespace ReactLiveSoldProject.ServerBL.Models.CustomerWallet
{
    public class Receipt
    {
        public Guid Id { get; set; }

        [Required]
        public Guid OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }

        [Required]
        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        // The single transaction that this receipt generated. Null until the receipt is posted.
        public Guid? WalletTransactionId { get; set; }
        public virtual WalletTransaction? WalletTransaction { get; set; }

        [Required]
        public TransactionType Type { get; set; } // Deposit or Withdrawal

        [Required]
        public decimal TotalAmount { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Required]
        public Guid CreatedByUserId { get; set; }
        public virtual User CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsPosted { get; set; } = false;
        public DateTime? PostedAt { get; set; }
        public Guid? PostedByUserId { get; set; }
        public virtual User? PostedByUser { get; set; }

        public virtual ICollection<ReceiptItem> Items { get; set; } = new List<ReceiptItem>();
    }
}
