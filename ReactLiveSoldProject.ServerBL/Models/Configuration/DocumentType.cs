namespace ReactLiveSoldProject.ServerBL.Models.Configuration
{
    /// <summary>
    /// Tipos de documentos que pueden usar numeración secuencial
    /// </summary>
    public enum DocumentType
    {
        Customer = 1,           // Clientes
        Vendor = 2,             // Proveedores
        SalesInvoice = 3,       // Facturas de venta
        SalesOrder = 4,         // Órdenes de venta
        PurchaseInvoice = 5,    // Facturas de compra
        PurchaseOrder = 6,      // Órdenes de compra
        PurchaseReceipt = 7,    // Recibos de compra
        Product = 8,            // Productos
        ProductVariant = 9,     // Variantes de producto
        Payment = 10,           // Pagos
        JournalEntry = 11,      // Asientos contables
        WalletTransaction = 12, // Transacciones de wallet
        StockMovement = 13,     // Movimientos de inventario
        Contact = 14            // Contactos
    }
}
