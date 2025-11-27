using Microsoft.EntityFrameworkCore;
using AutoMapper;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs.Purchases;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Purchases;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    /// <summary>
    /// 📄 VendorInvoiceService - Gestiona facturas de proveedores
    /// Las facturas se crean después de recibir mercancía (PurchaseReceipt)
    /// </summary>
    public class VendorInvoiceService : IVendorInvoiceService
    {
        private readonly LiveSoldDbContext _context;
        private readonly IMapper _mapper;

        public VendorInvoiceService(LiveSoldDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtiene lista de facturas de proveedores con filtros opcionales
        /// </summary>
        public async Task<List<VendorInvoiceDto>> GetVendorInvoicesAsync(
            Guid organizationId,
            Guid? vendorId = null,
            string? status = null)
        {
            var query = _context.VendorInvoices
                .Include(vi => vi.Vendor)
                    .ThenInclude(v => v.Contact)
                .Include(vi => vi.PurchaseReceipt)
                .Where(vi => vi.OrganizationId == organizationId);

            // Filtrar por proveedor
            if (vendorId.HasValue)
            {
                query = query.Where(vi => vi.VendorId == vendorId.Value);
            }

            // Filtrar por estado de pago
            if (!string.IsNullOrWhiteSpace(status) && status.ToLower() != "all")
            {
                if (Enum.TryParse<InvoicePaymentStatus>(status, true, out var paymentStatus))
                {
                    query = query.Where(vi => vi.PaymentStatus == paymentStatus);
                }
            }

            var invoices = await query
                .OrderByDescending(vi => vi.InvoiceDate)
                .ToListAsync();

            return _mapper.Map<List<VendorInvoiceDto>>(invoices);
        }

        /// <summary>
        /// Obtiene una factura por ID
        /// </summary>
        public async Task<VendorInvoiceDto?> GetVendorInvoiceByIdAsync(Guid invoiceId, Guid organizationId)
        {
            var invoice = await _context.VendorInvoices
                .Include(vi => vi.Vendor)
                    .ThenInclude(v => v.Contact)
                .Include(vi => vi.PurchaseReceipt)
                .FirstOrDefaultAsync(vi => vi.Id == invoiceId && vi.OrganizationId == organizationId);

            return _mapper.Map<VendorInvoiceDto>(invoice);
        }

        /// <summary>
        /// Crea una nueva factura de proveedor
        /// </summary>
        public async Task<VendorInvoiceDto> CreateVendorInvoiceAsync(
            Guid organizationId,
            Guid userId,
            CreateVendorInvoiceDto dto)
        {
            // Validar que el proveedor existe
            var vendor = await _context.Vendors
                .FirstOrDefaultAsync(v => v.Id == dto.VendorId && v.OrganizationId == organizationId);

            if (vendor == null)
                throw new Exception($"Proveedor con ID {dto.VendorId} no encontrado");

            // Validar que la recepción existe (si se especifica)
            if (dto.PurchaseReceiptId.HasValue)
            {
                var receipt = await _context.PurchaseReceipts
                    .FirstOrDefaultAsync(pr =>
                        pr.Id == dto.PurchaseReceiptId.Value &&
                        pr.OrganizationId == organizationId);

                if (receipt == null)
                    throw new Exception($"Recepción de compra con ID {dto.PurchaseReceiptId} no encontrada");

                if (receipt.VendorId != dto.VendorId)
                    throw new Exception("La recepción de compra no pertenece al proveedor especificado");
            }

            // Generar número de factura
            var invoiceNumber = await GenerateInvoiceNumberAsync(organizationId);

            var invoice = new VendorInvoice
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                InvoiceNumber = invoiceNumber,
                VendorInvoiceReference = dto.VendorInvoiceReference,
                PurchaseReceiptId = dto.PurchaseReceiptId,
                VendorId = dto.VendorId,
                InvoiceDate = DateTime.SpecifyKind(dto.InvoiceDate, DateTimeKind.Utc),
                DueDate = DateTime.SpecifyKind(dto.DueDate, DateTimeKind.Utc),
                Subtotal = dto.Subtotal,
                TaxAmount = dto.TaxAmount,
                TotalAmount = dto.TotalAmount,
                AmountPaid = 0,
                Status = InvoiceStatus.Pending,
                PaymentStatus = InvoicePaymentStatus.Unpaid,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.VendorInvoices.Add(invoice);
            await _context.SaveChangesAsync();

            return await GetVendorInvoiceByIdAsync(invoice.Id, organizationId)
                ?? throw new Exception("Error al recuperar la factura creada");
        }

        /// <summary>
        /// Actualiza una factura de proveedor
        /// </summary>
        public async Task<VendorInvoiceDto> UpdateVendorInvoiceAsync(
            Guid invoiceId,
            Guid organizationId,
            CreateVendorInvoiceDto dto)
        {
            var invoice = await _context.VendorInvoices
                .FirstOrDefaultAsync(vi => vi.Id == invoiceId && vi.OrganizationId == organizationId);

            if (invoice == null)
                throw new Exception($"Factura con ID {invoiceId} no encontrada");

            // No permitir actualizar facturas pagadas
            if (invoice.PaymentStatus == InvoicePaymentStatus.Paid)
                throw new Exception("No se puede actualizar una factura que ya está pagada");

            invoice.VendorInvoiceReference = dto.VendorInvoiceReference;
            invoice.InvoiceDate = DateTime.SpecifyKind(dto.InvoiceDate, DateTimeKind.Utc);
            invoice.DueDate = DateTime.SpecifyKind(dto.DueDate, DateTimeKind.Utc);
            invoice.Subtotal = dto.Subtotal;
            invoice.TaxAmount = dto.TaxAmount;
            invoice.TotalAmount = dto.TotalAmount;
            invoice.Notes = dto.Notes;
            invoice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetVendorInvoiceByIdAsync(invoice.Id, organizationId)
                ?? throw new Exception("Error al recuperar la factura actualizada");
        }

        /// <summary>
        /// Elimina una factura de proveedor (solo si no tiene pagos aplicados)
        /// </summary>
        public async Task DeleteVendorInvoiceAsync(Guid invoiceId, Guid organizationId)
        {
            var invoice = await _context.VendorInvoices
                .FirstOrDefaultAsync(vi => vi.Id == invoiceId && vi.OrganizationId == organizationId);

            if (invoice == null)
                throw new Exception($"Factura con ID {invoiceId} no encontrada");

            // No permitir eliminar facturas con pagos aplicados
            if (invoice.AmountPaid > 0)
                throw new Exception("No se puede eliminar una factura que tiene pagos aplicados");

            _context.VendorInvoices.Remove(invoice);
            await _context.SaveChangesAsync();
        }

        #region Métodos Auxiliares

        /// <summary>
        /// Genera un número secuencial para la factura
        /// </summary>
        private async Task<string> GenerateInvoiceNumberAsync(Guid organizationId)
        {
            var year = DateTime.UtcNow.Year;
            var lastInvoice = await _context.VendorInvoices
                .Where(vi => vi.OrganizationId == organizationId && vi.InvoiceNumber.StartsWith($"VINV-{year}-"))
                .OrderByDescending(vi => vi.InvoiceNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastInvoice != null)
            {
                var parts = lastInvoice.InvoiceNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"VINV-{year}-{nextNumber:D6}";
        }

        #endregion
    }
}
