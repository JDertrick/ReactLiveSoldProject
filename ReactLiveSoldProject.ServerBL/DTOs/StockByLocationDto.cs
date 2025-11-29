namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class StockByLocationDto
    {
        public Guid LocationId { get; set; }
        public string LocationName { get; set; }
        public int Quantity { get; set; }
    }
}
