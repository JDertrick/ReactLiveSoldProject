using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReactLiveSoldProject.Server.Controllers.Base;
using ReactLiveSoldProject.ServerBL.DTOs.Payments;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;

namespace ReactLiveSoldProject.Server.Controllers
{
    /// <summary>
    /// 💰 PaymentController - Gestiona pagos a proveedores
    ///
    /// ENDPOINTS CRÍTICOS:
    /// - POST /api/payment: Crea pago y genera PaymentApplications + JournalEntry
    /// - POST /api/payment/{id}/void: Anula pago y revierte aplicaciones
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Employee")]
    public class PaymentController : BaseController
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// Obtiene todos los pagos con filtros opcionales
        /// </summary>
        /// <param name="vendorId">Filtrar por proveedor</param>
        /// <param name="searchTerm">Término de búsqueda</param>
        [HttpGet]
        public async Task<IActionResult> GetPayments(
            [FromQuery] Guid? vendorId = null,
            [FromQuery] string? searchTerm = null)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                var payments = await _paymentService.GetPaymentsAsync(
                    organizationId.Value,
                    vendorId,
                    searchTerm);

                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener pagos", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un pago por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayment(Guid id)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                var payment = await _paymentService.GetPaymentByIdAsync(id, organizationId.Value);
                if (payment == null)
                    return NotFound(new { message = "Pago no encontrado" });

                return Ok(payment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener pago", error = ex.Message });
            }
        }

        /// <summary>
        /// ⚡ ACCIÓN CRÍTICA: Crea un pago a proveedor
        ///
        /// Genera automáticamente:
        /// - PaymentApplication(s) a facturas especificadas
        /// - Actualiza AmountPaid y PaymentStatus de VendorInvoice(s)
        /// - Actualiza CurrentBalance de CompanyBankAccount
        /// - JournalEntry: DEBE Cuentas por Pagar / HABER Banco
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto)
        {
            var organizationId = GetOrganizationId();
            var userId = GetUserId();

            if (organizationId == null || userId == null)
                return Unauthorized(new { message = "OrganizationId o UserId no encontrado en el token" });

            try
            {
                var payment = await _paymentService.CreatePaymentAsync(
                    organizationId.Value,
                    userId.Value,
                    dto);

                return CreatedAtAction(
                    nameof(GetPayment),
                    new { id = payment.Id },
                    payment);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al crear pago", error = ex.Message });
            }
        }

        /// <summary>
        /// ⚡ ACCIÓN CRÍTICA: Anula un pago
        ///
        /// Revierte automáticamente:
        /// - PaymentApplications (reduce AmountPaid de facturas)
        /// - Actualiza PaymentStatus de VendorInvoice(s)
        /// - Restaura CurrentBalance de CompanyBankAccount
        /// - Marca el pago como Void
        /// </summary>
        [HttpPost("{id}/void")]
        public async Task<IActionResult> VoidPayment(Guid id, [FromBody] VoidPaymentDto dto)
        {
            var organizationId = GetOrganizationId();
            var userId = GetUserId();

            if (organizationId == null || userId == null)
                return Unauthorized(new { message = "OrganizationId o UserId no encontrado en el token" });

            try
            {
                var payment = await _paymentService.VoidPaymentAsync(
                    id,
                    organizationId.Value,
                    userId.Value,
                    dto);

                return Ok(new
                {
                    message = "Pago anulado exitosamente",
                    payment
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al anular pago", error = ex.Message });
            }
        }
    }
}
