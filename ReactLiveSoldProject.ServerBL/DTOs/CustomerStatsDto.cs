namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class CustomerStatsDto
    {
        public decimal TotalWalletSum { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public int TotalCustomers { get; set; }
    }
}
