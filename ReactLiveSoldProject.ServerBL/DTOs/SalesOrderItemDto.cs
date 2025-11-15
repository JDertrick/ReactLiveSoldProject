namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class SalesOrderItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductVariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? VariantSku { get; set; }
        public string? VariantAttributes { get; set; }
        public int Quantity { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitCost { get; set; }
        public decimal Subtotal { get; set; }
        public decimal GrossProfit { get; set; } // (UnitPrice - UnitCost) * Quantity
        public decimal ProfitMargin { get; set; } // (GrossProfit / Subtotal) * 100
        public string? ItemDescription { get; set; }
    }
}
