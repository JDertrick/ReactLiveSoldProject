namespace ReactLiveSoldProject.ServerBL.Models.Sales
{
    public class SalesSetup
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }

        // Foreign Keys a las series (Strings en BC, IDs o Codes aquí)
        public string? CustomerNos { get; set; }  // Code de NoSeries para Clientes
        public string? OrderNos { get; set; }     // Code de NoSeries para Pedidos
        public string? InvoiceNos { get; set; }   // Code de NoSeries para Facturas
        public string? QuoteNos { get; set; }     // Code de NoSeries para Cotizaciones
    }
}