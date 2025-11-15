namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class WalletTransactionDto
    {
        public Guid Id { get; set; }
        public Guid WalletId { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public Guid? RelatedSalesOrderId { get; set; }
        public Guid? AuthorizedByUserId { get; set; }
        public string? AuthorizedByUserName { get; set; }
        public string? Notes { get; set; }
        public string? Reference { get; set; }
        public bool IsPosted { get; set; }
        public decimal? BalanceBefore { get; set; }
        public decimal? BalanceAfter { get; set; }
        public Guid? PostedByUserId { get; set; }
        public string? PostedByUserName { get; set; }
        public DateTime? PostedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
