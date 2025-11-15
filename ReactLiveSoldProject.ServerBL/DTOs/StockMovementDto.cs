namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class StockMovementDto
    {
        public Guid Id { get; set; }
        public Guid ProductVariantId { get; set; }
        public string ProductName { get; set; }
        public string VariantSku { get; set; }
        public string MovementType { get; set; }
        public int Quantity { get; set; }
        public int StockBefore { get; set; }
        public int StockAfter { get; set; }
        public Guid? RelatedSalesOrderId { get; set; }
        public string? CreatedByUserName { get; set; }
        public string? Notes { get; set; }
        public string? Reference { get; set; }
        public decimal? UnitCost { get; set; }
        public bool IsPosted { get; set; }
        public DateTime? PostedAt { get; set; }
        public string? PostedByUserName { get; set; }
        public bool IsRejected { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectedByUserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
