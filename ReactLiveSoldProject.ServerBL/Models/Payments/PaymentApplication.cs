using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Models.Purchases;

namespace ReactLiveSoldProject.ServerBL.Models.Payments
{
    /// <summary>
    /// Aplicación de un pago a una o más facturas de proveedor
    /// Permite pagos parciales y distribución de un pago entre múltiples facturas
    /// </summary>
    public class PaymentApplication
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El ID del pago es obligatorio")]
        public Guid PaymentId { get; set; }
        public virtual Payment Payment { get; set; }

        [Required(ErrorMessage = "El ID de la factura es obligatorio")]
        public Guid VendorInvoiceId { get; set; }
        public virtual VendorInvoice VendorInvoice { get; set; }

        [Required(ErrorMessage = "El monto aplicado es obligatorio")]
        public decimal AmountApplied { get; set; } // Monto de este pago aplicado a esta factura

        public decimal DiscountTaken { get; set; } = 0; // Descuento por pronto pago tomado

        [Required(ErrorMessage = "La fecha de aplicación es obligatoria")]
        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
