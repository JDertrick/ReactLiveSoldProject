using System;
using System.Collections.Generic;
using ReactLiveSoldProject.ServerBL.Base;

namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class ReceiptDto
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public Guid? WalletTransactionId { get; set; }
        public TransactionType Type { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPosted { get; set; }
        public DateTime? PostedAt { get; set; }
        public Guid? PostedByUserId { get; set; }
        public string? PostedByUserName { get; set; }
        public List<ReceiptItemDto> Items { get; set; } = new List<ReceiptItemDto>();
    }

    public class ReceiptItemDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
    }
}
