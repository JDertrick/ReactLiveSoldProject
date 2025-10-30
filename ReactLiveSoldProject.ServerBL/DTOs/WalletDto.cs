namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class WalletDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
