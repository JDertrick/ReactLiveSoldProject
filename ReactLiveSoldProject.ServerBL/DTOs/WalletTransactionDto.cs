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
        public DateTime CreatedAt { get; set; }
    }
}
