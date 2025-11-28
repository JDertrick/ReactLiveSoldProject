using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs.Payments;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Accounting;
using ReactLiveSoldProject.ServerBL.Models.Vendors;
using ReactLiveSoldProject.ServerBL.Models.Purchases;
using ReactLiveSoldProject.ServerBL.Models.Banking;
using ReactLiveSoldProject.ServerBL.Models.Payments;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    /// <summary>
    /// 💰 PaymentService - Gestiona pagos a proveedores con contabilidad automática
    ///
    /// FUNCIONALIDAD CRÍTICA:
    /// - CreatePaymentAsync: Aplica pagos a facturas y genera asiento contable automático
    ///   (DEBE: Cuentas por Pagar / HABER: Banco)
    /// - VoidPaymentAsync: Anula pagos y revierte aplicaciones a facturas
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly LiveSoldDbContext _context;

        public PaymentService(LiveSoldDbContext context)
        {
            _context = context;
        }

        #region CRUD Básico

        /// <summary>
        /// Obtiene lista de pagos con filtros opcionales
        /// </summary>
        public async Task<List<PaymentDto>> GetPaymentsAsync(
            Guid organizationId,
            Guid? vendorId = null,
            string? searchTerm = null)
        {
            var query = _context.Payments
                .Include(p => p.Vendor)
                    .ThenInclude(v => v.Contact)
                .Include(p => p.CompanyBankAccount)
                .Include(p => p.VendorBankAccount)
                .Include(p => p.CreatedByUser)
                .Include(p => p.Applications)
                    .ThenInclude(pa => pa.VendorInvoice)
                .Where(p => p.OrganizationId == organizationId);

            // Filtrar por proveedor si se especifica
            if (vendorId.HasValue)
            {
                query = query.Where(p => p.VendorId == vendorId.Value);
            }

            // Filtrar por término de búsqueda
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower().Trim();
                query = query.Where(p =>
                    p.PaymentNumber.ToLower().Contains(lowerSearchTerm) ||
                    (p.ReferenceNumber != null && p.ReferenceNumber.ToLower().Contains(lowerSearchTerm)) ||
                    (p.Vendor.Contact.FirstName != null && p.Vendor.Contact.FirstName.ToLower().Contains(lowerSearchTerm)) ||
                    (p.Vendor.Contact.LastName != null && p.Vendor.Contact.LastName.ToLower().Contains(lowerSearchTerm)) ||
                    (p.Vendor.Contact.Company != null && p.Vendor.Contact.Company.ToLower().Contains(lowerSearchTerm))
                );
            }

            var payments = await query
                .OrderByDescending(p => p.PaymentDate)
                .ThenByDescending(p => p.CreatedAt)
                .ToListAsync();

            return payments.Select(p => MapToDto(p)).ToList();
        }

        /// <summary>
        /// Obtiene un pago específico por ID
        /// </summary>
        public async Task<PaymentDto?> GetPaymentByIdAsync(Guid paymentId, Guid organizationId)
        {
            var payment = await _context.Payments
                .Include(p => p.Vendor)
                    .ThenInclude(v => v.Contact)
                .Include(p => p.CompanyBankAccount)
                .Include(p => p.VendorBankAccount)
                .Include(p => p.CreatedByUser)
                .Include(p => p.Applications)
                    .ThenInclude(pa => pa.VendorInvoice)
                .FirstOrDefaultAsync(p => p.Id == paymentId && p.OrganizationId == organizationId);

            if (payment == null)
                return null;

            return MapToDto(payment);
        }

        #endregion

        #region ⚡ MÉTODO CRÍTICO: CreatePaymentAsync

        /// <summary>
        /// ⚡ MÉTODO CRÍTICO: Crea un pago y ejecuta toda la lógica automática
        ///
        /// PROCESO:
        /// 1. Genera número de pago secuencial
        /// 2. Crea el registro Payment
        /// 3. Aplica el pago a las facturas especificadas (PaymentApplication)
        /// 4. Actualiza AmountPaid y PaymentStatus de cada VendorInvoice
        /// 5. Actualiza CurrentBalance de CompanyBankAccount
        /// 6. Genera asiento contable automático:
        ///    DEBE: Cuentas por Pagar (reduce el pasivo)
        ///    HABER: Banco (reduce el activo)
        /// </summary>
        public async Task<PaymentDto> CreatePaymentAsync(
            Guid organizationId,
            Guid userId,
            CreatePaymentDto dto)
        {
            try
            {
                // 1. Validar que el vendor existe
                var vendor = await ValidateVendorExists(dto.VendorId, organizationId);

                // 2. Validar que la cuenta bancaria de la empresa existe
                var companyBankAccount = await ValidateCompanyBankAccount(dto.CompanyBankAccountId, organizationId);

                // 3. Validar que hay suficiente saldo en la cuenta bancaria
                ValidateBankAccountBalance(companyBankAccount, dto.AmountPaid);

                // 4. Validar las aplicaciones de pago a facturas
                await ValidatePaymentApplications(dto.InvoiceApplications, dto.VendorId, dto.AmountPaid, organizationId);

                // 5. Generar número de pago
                var paymentNumber = await GeneratePaymentNumberAsync(organizationId);

                // 6. Crear el registro Payment con estado Pending
                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    PaymentNumber = paymentNumber,
                    PaymentDate = DateTime.SpecifyKind(dto.PaymentDate, DateTimeKind.Utc),
                    VendorId = dto.VendorId,
                    PaymentMethod = dto.PaymentMethod,
                    CompanyBankAccountId = dto.CompanyBankAccountId,
                    VendorBankAccountId = dto.VendorBankAccountId,
                    AmountPaid = dto.AmountPaid,
                    Currency = dto.Currency,
                    ExchangeRate = dto.ExchangeRate,
                    ReferenceNumber = dto.ReferenceNumber,
                    Notes = dto.Notes,
                    Status = PaymentStatus.Pending, // ⚠️ CAMBIO: Se crea como Pending
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Payments.Add(payment);

                // 7. Crear PaymentApplications (sin aplicar aún a facturas)
                foreach (var app in dto.InvoiceApplications)
                {
                    var paymentApplication = new PaymentApplication
                    {
                        Id = Guid.NewGuid(),
                        PaymentId = payment.Id,
                        VendorInvoiceId = app.VendorInvoiceId,
                        AmountApplied = app.AmountApplied,
                        DiscountTaken = app.DiscountTaken,
                        ApplicationDate = DateTime.UtcNow
                    };
                    _context.PaymentApplications.Add(paymentApplication);
                }

                await _context.SaveChangesAsync();

                return await GetPaymentByIdAsync(payment.Id, organizationId)
                    ?? throw new Exception("Error al recuperar el pago creado");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// ⚡ Aprueba un pago (Pending → Approved)
        /// </summary>
        public async Task<PaymentDto> ApprovePaymentAsync(
            Guid paymentId,
            Guid organizationId,
            Guid userId)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId && p.OrganizationId == organizationId);

            if (payment == null)
                throw new Exception("Pago no encontrado");

            if (payment.Status != PaymentStatus.Pending)
                throw new Exception($"El pago debe estar en estado Pending para ser aprobado. Estado actual: {payment.Status}");

            payment.Status = PaymentStatus.Approved;
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetPaymentByIdAsync(paymentId, organizationId)
                ?? throw new Exception("Error al recuperar el pago aprobado");
        }

        /// <summary>
        /// ⚡ Rechaza un pago (Pending → Rejected)
        /// </summary>
        public async Task<PaymentDto> RejectPaymentAsync(
            Guid paymentId,
            Guid organizationId,
            Guid userId,
            string reason)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId && p.OrganizationId == organizationId);

            if (payment == null)
                throw new Exception("Pago no encontrado");

            if (payment.Status != PaymentStatus.Pending)
                throw new Exception($"El pago debe estar en estado Pending para ser rechazado. Estado actual: {payment.Status}");

            payment.Status = PaymentStatus.Rejected;
            payment.Notes = string.IsNullOrEmpty(payment.Notes)
                ? $"Rechazado: {reason}"
                : $"{payment.Notes}\n\nRechazado: {reason}";
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetPaymentByIdAsync(paymentId, organizationId)
                ?? throw new Exception("Error al recuperar el pago rechazado");
        }

        /// <summary>
        /// ⚡ MÉTODO CRÍTICO: Contabiliza un pago (Approved → Posted)
        ///
        /// PROCESO:
        /// 1. Aplica el pago a las facturas (actualiza AmountPaid y PaymentStatus)
        /// 2. Actualiza saldo de CompanyBankAccount
        /// 3. Genera asiento contable: DEBE Cuentas por Pagar / HABER Banco
        /// </summary>
        public async Task<PaymentDto> PostPaymentAsync(
            Guid paymentId,
            Guid organizationId,
            Guid userId)
        {
            try
            {
                // 1. Obtener el pago con todas sus aplicaciones
                var payment = await _context.Payments
                    .Include(p => p.Applications)
                        .ThenInclude(pa => pa.VendorInvoice)
                    .Include(p => p.CompanyBankAccount)
                    .FirstOrDefaultAsync(p => p.Id == paymentId && p.OrganizationId == organizationId);

                if (payment == null)
                    throw new Exception("Pago no encontrado");

                if (payment.Status != PaymentStatus.Approved)
                    throw new Exception($"El pago debe estar en estado Approved para ser contabilizado. Estado actual: {payment.Status}");

                // 2. Aplicar el pago a las facturas
                foreach (var app in payment.Applications)
                {
                    await UpdateVendorInvoicePaymentStatus(
                        app.VendorInvoiceId,
                        app.AmountApplied + app.DiscountTaken);
                }

                // 3. Actualizar saldo de cuenta bancaria
                await UpdateCompanyBankAccountBalance(payment.CompanyBankAccountId, -payment.AmountPaid);

                // 4. Generar asiento contable automático
                // Buscar cuenta de Cuentas por Pagar (Accounts Payable)
                var glAccountsPayable = await _context.ChartOfAccounts
                    .FirstOrDefaultAsync(coa =>
                        coa.OrganizationId == organizationId &&
                        coa.AccountType == AccountType.Liability &&
                        (coa.AccountName.Contains("Cuentas por Pagar") ||
                         coa.AccountName.Contains("Accounts Payable") ||
                         coa.AccountName.Contains("Proveedores") ||
                         coa.AccountCode.StartsWith("2")));

                if (glAccountsPayable == null)
                    throw new Exception("No se encontró una cuenta contable de Cuentas por Pagar (Pasivo). " +
                        "Por favor, cree una cuenta de tipo 'Pasivo' con el nombre 'Cuentas por Pagar' o que su código comience con '2'.");

                var journalEntry = await GeneratePaymentJournalEntry(
                    payment,
                    organizationId,
                    userId,
                    glAccountsPayable.Id);

                // 5. Vincular el asiento contable al pago
                payment.PaymentJournalEntryId = journalEntry.Id;
                payment.Status = PaymentStatus.Posted;
                payment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return await GetPaymentByIdAsync(paymentId, organizationId)
                    ?? throw new Exception("Error al recuperar el pago contabilizado");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Métodos Auxiliares: Validación

        /// <summary>
        /// Valida que el proveedor existe y pertenece a la organización
        /// </summary>
        private async Task<Vendor> ValidateVendorExists(Guid vendorId, Guid organizationId)
        {
            var vendor = await _context.Vendors
                .FirstOrDefaultAsync(v => v.Id == vendorId && v.OrganizationId == organizationId);

            if (vendor == null)
                throw new Exception($"Proveedor con ID {vendorId} no encontrado");

            return vendor;
        }

        /// <summary>
        /// Valida que la cuenta bancaria de la empresa existe
        /// </summary>
        private async Task<CompanyBankAccount> ValidateCompanyBankAccount(
            Guid companyBankAccountId,
            Guid organizationId)
        {
            var account = await _context.CompanyBankAccounts
                .Include(cba => cba.GLAccount)
                .FirstOrDefaultAsync(cba =>
                    cba.Id == companyBankAccountId &&
                    cba.OrganizationId == organizationId);

            if (account == null)
                throw new Exception($"Cuenta bancaria de empresa con ID {companyBankAccountId} no encontrada");

            if (!account.IsActive)
                throw new Exception("La cuenta bancaria está inactiva");

            return account;
        }

        /// <summary>
        /// Valida que hay suficiente saldo en la cuenta bancaria
        /// </summary>
        private void ValidateBankAccountBalance(CompanyBankAccount account, decimal amountToPay)
        {
            if (account.CurrentBalance < amountToPay)
            {
                throw new Exception(
                    $"Saldo insuficiente en cuenta bancaria. " +
                    $"Saldo actual: {account.CurrentBalance:C}, Monto a pagar: {amountToPay:C}");
            }
        }

        /// <summary>
        /// Valida que las aplicaciones de pago son correctas
        /// </summary>
        private async Task ValidatePaymentApplications(
            List<PaymentInvoiceApplicationDto> applications,
            Guid vendorId,
            decimal totalPaymentAmount,
            Guid organizationId)
        {
            if (applications == null || applications.Count == 0)
                throw new Exception("Debe especificar al menos una factura a pagar");

            // Validar que la suma de aplicaciones no excede el monto del pago
            var totalApplied = applications.Sum(a => a.AmountApplied + a.DiscountTaken);
            if (totalApplied > totalPaymentAmount)
            {
                throw new Exception(
                    $"La suma de aplicaciones ({totalApplied:C}) excede el monto del pago ({totalPaymentAmount:C})");
            }

            // Validar cada factura
            foreach (var app in applications)
            {
                var invoice = await _context.VendorInvoices
                    .FirstOrDefaultAsync(vi =>
                        vi.Id == app.VendorInvoiceId &&
                        vi.OrganizationId == organizationId);

                if (invoice == null)
                    throw new Exception($"Factura con ID {app.VendorInvoiceId} no encontrada");

                if (invoice.VendorId != vendorId)
                    throw new Exception($"La factura {invoice.InvoiceNumber} no pertenece al proveedor especificado");

                if (invoice.Status == InvoiceStatus.Cancelled)
                    throw new Exception($"La factura {invoice.InvoiceNumber} está cancelada");

                // Validar que no se excede el monto pendiente
                var amountDue = invoice.TotalAmount - invoice.AmountPaid;
                if (app.AmountApplied + app.DiscountTaken > amountDue)
                {
                    throw new Exception(
                        $"La factura {invoice.InvoiceNumber} tiene un saldo de {amountDue:C}, " +
                        $"pero se intenta aplicar {app.AmountApplied + app.DiscountTaken:C}");
                }
            }
        }

        #endregion

        #region Métodos Auxiliares: Aplicar Pago a Facturas

        /// <summary>
        /// Crea los registros PaymentApplication y actualiza las facturas
        /// </summary>
        private async Task ApplyPaymentToInvoices(
            Guid paymentId,
            List<PaymentInvoiceApplicationDto> applications)
        {
            foreach (var app in applications)
            {
                // Crear el registro PaymentApplication
                var paymentApplication = new PaymentApplication
                {
                    Id = Guid.NewGuid(),
                    PaymentId = paymentId,
                    VendorInvoiceId = app.VendorInvoiceId,
                    AmountApplied = app.AmountApplied,
                    DiscountTaken = app.DiscountTaken,
                    ApplicationDate = DateTime.UtcNow
                };

                _context.PaymentApplications.Add(paymentApplication);

                // Actualizar la factura
                await UpdateVendorInvoicePaymentStatus(
                    app.VendorInvoiceId,
                    app.AmountApplied + app.DiscountTaken);
            }
        }

        /// <summary>
        /// Actualiza el AmountPaid y PaymentStatus de una factura
        /// </summary>
        private async Task UpdateVendorInvoicePaymentStatus(
            Guid vendorInvoiceId,
            decimal amountToApply)
        {
            var invoice = await _context.VendorInvoices
                .FirstOrDefaultAsync(vi => vi.Id == vendorInvoiceId);

            if (invoice == null)
                throw new Exception($"Factura con ID {vendorInvoiceId} no encontrada");

            // Actualizar monto pagado
            invoice.AmountPaid += amountToApply;
            invoice.UpdatedAt = DateTime.UtcNow;

            // Actualizar estado de pago
            if (invoice.AmountPaid >= invoice.TotalAmount)
            {
                invoice.PaymentStatus = InvoicePaymentStatus.Paid;
            }
            else if (invoice.AmountPaid > 0)
            {
                invoice.PaymentStatus = InvoicePaymentStatus.Partial;
            }
            else
            {
                invoice.PaymentStatus = InvoicePaymentStatus.Unpaid;
            }
        }

        #endregion

        #region Métodos Auxiliares: Actualizar Cuenta Bancaria

        /// <summary>
        /// Actualiza el saldo de la cuenta bancaria de la empresa
        /// </summary>
        private async Task UpdateCompanyBankAccountBalance(
            Guid companyBankAccountId,
            decimal amountChange)
        {
            var account = await _context.CompanyBankAccounts
                .FirstOrDefaultAsync(cba => cba.Id == companyBankAccountId);

            if (account == null)
                throw new Exception($"Cuenta bancaria con ID {companyBankAccountId} no encontrada");

            account.CurrentBalance += amountChange;
            account.UpdatedAt = DateTime.UtcNow;
        }

        #endregion

        #region Métodos Auxiliares: Asiento Contable Automático

        /// <summary>
        /// Genera el asiento contable automático para el pago
        ///
        /// LÓGICA CONTABLE:
        /// DEBE:  Cuentas por Pagar (reduce el pasivo)
        /// HABER: Banco (reduce el activo)
        /// </summary>
        private async Task<JournalEntry> GeneratePaymentJournalEntry(
            Payment payment,
            Guid organizationId,
            Guid userId,
            Guid? glAccountsPayableId)
        {
            // 1. Obtener o buscar la cuenta de Cuentas por Pagar
            var accountsPayableAccount = await GetOrCreateAccountsPayableAccount(
                organizationId,
                glAccountsPayableId);

            // 2. Obtener la cuenta contable del banco (ya está vinculada)
            var companyBankAccount = await _context.CompanyBankAccounts
                .Include(cba => cba.GLAccount)
                .FirstOrDefaultAsync(cba => cba.Id == payment.CompanyBankAccountId);

            if (companyBankAccount?.GLAccount == null)
                throw new Exception("La cuenta bancaria no tiene cuenta contable vinculada");

            var bankAccount = companyBankAccount.GLAccount;

            // 3. Crear el asiento contable
            var journalEntry = new JournalEntry
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                EntryNumber = await GenerateJournalEntryNumberAsync(organizationId),
                EntryDate = payment.PaymentDate,
                Description = $"Pago a proveedor {payment.Vendor?.Contact?.Company ?? payment.VendorId.ToString()} - {payment.PaymentNumber}",
                DocumentType = "PAYMENT",
                DocumentNumber = payment.PaymentNumber,
                VendorId = payment.VendorId,
                Status = "Posted",
                PostedBy = userId,
                PostedDate = DateTime.UtcNow,
                Currency = payment.Currency,
                ExchangeRate = payment.ExchangeRate,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.JournalEntries.Add(journalEntry);

            // 4. Línea 1: DEBE - Cuentas por Pagar (reduce el pasivo)
            var debitLine = new JournalEntryLine
            {
                Id = Guid.NewGuid(),
                JournalEntryId = journalEntry.Id,
                LineNumber = 1,
                AccountId = accountsPayableAccount.Id,
                Description = $"Pago a {payment.Vendor?.Contact?.Company ?? "proveedor"}",
                DebitAmount = payment.AmountPaid,
                CreditAmount = 0,
                VendorId = payment.VendorId
            };

            _context.JournalEntryLines.Add(debitLine);

            // 5. Línea 2: HABER - Banco (reduce el activo)
            var creditLine = new JournalEntryLine
            {
                Id = Guid.NewGuid(),
                JournalEntryId = journalEntry.Id,
                LineNumber = 2,
                AccountId = bankAccount.Id,
                Description = $"Pago mediante {payment.PaymentMethod}",
                DebitAmount = 0,
                CreditAmount = payment.AmountPaid,
                VendorId = payment.VendorId
            };

            _context.JournalEntryLines.Add(creditLine);

            return journalEntry;
        }

        /// <summary>
        /// Obtiene o busca la cuenta de Cuentas por Pagar
        /// </summary>
        private async Task<ChartOfAccount> GetOrCreateAccountsPayableAccount(
            Guid organizationId,
            Guid? providedAccountId)
        {
            // Si se proporcionó un ID, intentar usarlo
            if (providedAccountId.HasValue)
            {
                var account = await _context.ChartOfAccounts
                    .FirstOrDefaultAsync(coa =>
                        coa.Id == providedAccountId.Value &&
                        coa.OrganizationId == organizationId);

                if (account != null)
                    return account;
            }

            // Buscar cuenta del sistema tipo AccountsPayable
            var systemAccount = await _context.ChartOfAccounts
                .FirstOrDefaultAsync(coa =>
                    coa.OrganizationId == organizationId &&
                    coa.SystemAccountType == SystemAccountType.AccountsPayable &&
                    coa.IsActive);

            if (systemAccount != null)
                return systemAccount;

            throw new Exception(
                "No se encontró cuenta de Cuentas por Pagar. " +
                "Por favor, configure una cuenta con tipo AccountsPayable en el catálogo de cuentas.");
        }

        #endregion

        #region ⚡ MÉTODO CRÍTICO: VoidPaymentAsync

        /// <summary>
        /// ⚡ MÉTODO CRÍTICO: Anula un pago y revierte toda la lógica
        ///
        /// PROCESO:
        /// 1. Valida que el pago puede ser anulado
        /// 2. Revierte las aplicaciones a facturas (reduce AmountPaid)
        /// 3. Actualiza PaymentStatus de las facturas
        /// 4. Restaura el saldo de CompanyBankAccount
        /// 5. Marca el pago como Void
        /// 6. Genera asiento contable de reversión (opcional)
        /// </summary>
        public async Task<PaymentDto> VoidPaymentAsync(
            Guid paymentId,
            Guid organizationId,
            Guid userId,
            VoidPaymentDto dto)
        {
            // 1. Obtener el pago con todas sus relaciones
            var payment = await GetPaymentForVoiding(paymentId, organizationId);

            // 2. Validar que el pago puede ser anulado
            ValidatePaymentCanBeVoided(payment);

            // 3. Revertir aplicaciones a facturas
            await ReversePaymentApplications(payment.Applications);

            // 4. Restaurar saldo de cuenta bancaria
            await UpdateCompanyBankAccountBalance(payment.CompanyBankAccountId, payment.AmountPaid);

            // 5. Marcar el pago como anulado
            payment.Status = PaymentStatus.Void;
            payment.Notes = $"[ANULADO] {dto.Reason ?? "Sin razón especificada"}\n{payment.Notes}";
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetPaymentByIdAsync(payment.Id, organizationId)
                ?? throw new Exception("Error al recuperar el pago anulado");
        }

        /// <summary>
        /// Obtiene el pago con todas sus relaciones para anulación
        /// </summary>
        private async Task<Payment> GetPaymentForVoiding(Guid paymentId, Guid organizationId)
        {
            var payment = await _context.Payments
                .Include(p => p.Applications)
                    .ThenInclude(pa => pa.VendorInvoice)
                .Include(p => p.Vendor)
                    .ThenInclude(v => v.Contact)
                .FirstOrDefaultAsync(p => p.Id == paymentId && p.OrganizationId == organizationId);

            if (payment == null)
                throw new Exception($"Pago con ID {paymentId} no encontrado");

            return payment;
        }

        /// <summary>
        /// Valida que el pago puede ser anulado
        /// </summary>
        private void ValidatePaymentCanBeVoided(Payment payment)
        {
            if (payment.Status == PaymentStatus.Void)
                throw new Exception("El pago ya está anulado");

            // Aquí podrías agregar validaciones adicionales, por ejemplo:
            // - No permitir anular pagos de períodos cerrados
            // - Requerir aprobación de supervisor
            // - Verificar que las facturas no estén en un proceso de auditoría
        }

        /// <summary>
        /// Revierte las aplicaciones de pago a facturas
        /// </summary>
        private async Task ReversePaymentApplications(ICollection<PaymentApplication> applications)
        {
            foreach (var app in applications)
            {
                var invoice = await _context.VendorInvoices
                    .FirstOrDefaultAsync(vi => vi.Id == app.VendorInvoiceId);

                if (invoice != null)
                {
                    // Restar el monto aplicado
                    invoice.AmountPaid -= (app.AmountApplied + app.DiscountTaken);
                    invoice.UpdatedAt = DateTime.UtcNow;

                    // Actualizar estado de pago
                    if (invoice.AmountPaid >= invoice.TotalAmount)
                    {
                        invoice.PaymentStatus = InvoicePaymentStatus.Paid;
                    }
                    else if (invoice.AmountPaid > 0)
                    {
                        invoice.PaymentStatus = InvoicePaymentStatus.Partial;
                    }
                    else
                    {
                        invoice.PaymentStatus = InvoicePaymentStatus.Unpaid;
                    }
                }
            }
        }

        #endregion

        #region Métodos Auxiliares: Generación de Números

        /// <summary>
        /// Genera un número secuencial para el pago
        /// </summary>
        private async Task<string> GeneratePaymentNumberAsync(Guid organizationId)
        {
            var year = DateTime.UtcNow.Year;
            var lastPayment = await _context.Payments
                .Where(p => p.OrganizationId == organizationId && p.PaymentNumber.StartsWith($"PAY-{year}-"))
                .OrderByDescending(p => p.PaymentNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastPayment != null)
            {
                var parts = lastPayment.PaymentNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"PAY-{year}-{nextNumber:D6}";
        }

        /// <summary>
        /// Genera un número secuencial para el asiento contable
        /// </summary>
        private async Task<string> GenerateJournalEntryNumberAsync(Guid organizationId)
        {
            var year = DateTime.UtcNow.Year;
            var lastEntry = await _context.JournalEntries
                .Where(je => je.OrganizationId == organizationId && je.EntryNumber.StartsWith($"JE-{year}-"))
                .OrderByDescending(je => je.EntryNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastEntry != null)
            {
                var parts = lastEntry.EntryNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"JE-{year}-{nextNumber:D6}";
        }

        #endregion

        #region Mapeo DTO

        /// <summary>
        /// Mapea la entidad Payment a PaymentDto
        /// </summary>
        private PaymentDto MapToDto(Payment payment)
        {
            return new PaymentDto
            {
                Id = payment.Id,
                OrganizationId = payment.OrganizationId,
                PaymentNumber = payment.PaymentNumber,
                PaymentDate = payment.PaymentDate,
                VendorId = payment.VendorId,
                VendorName = payment.Vendor?.Contact != null
                    ? (payment.Vendor.Contact.Company ??
                       $"{payment.Vendor.Contact.FirstName} {payment.Vendor.Contact.LastName}".Trim())
                    : null,
                PaymentMethod = payment.PaymentMethod,
                CompanyBankAccountId = payment.CompanyBankAccountId,
                CompanyBankAccountName = payment.CompanyBankAccount != null
                    ? $"{payment.CompanyBankAccount.BankName} - {payment.CompanyBankAccount.AccountNumber}"
                    : null,
                VendorBankAccountId = payment.VendorBankAccountId,
                VendorBankAccountName = payment.VendorBankAccount != null
                    ? $"{payment.VendorBankAccount.BankName} - {payment.VendorBankAccount.AccountNumber}"
                    : null,
                AmountPaid = payment.AmountPaid,
                Currency = payment.Currency,
                ExchangeRate = payment.ExchangeRate,
                ReferenceNumber = payment.ReferenceNumber,
                Notes = payment.Notes,
                Status = payment.Status,
                PaymentJournalEntryId = payment.PaymentJournalEntryId,
                CreatedBy = payment.CreatedBy,
                CreatedByName = payment.CreatedByUser != null
                    ? $"{payment.CreatedByUser.FirstName} {payment.CreatedByUser.LastName}".Trim()
                    : null,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt,
                Applications = payment.Applications?.Select(pa => new PaymentApplicationDto
                {
                    Id = pa.Id,
                    PaymentId = pa.PaymentId,
                    VendorInvoiceId = pa.VendorInvoiceId,
                    VendorInvoiceNumber = pa.VendorInvoice?.InvoiceNumber,
                    InvoiceNumber = pa.VendorInvoice?.InvoiceNumber,
                    VendorInvoiceReference = pa.VendorInvoice?.VendorInvoiceReference,
                    AmountApplied = pa.AmountApplied,
                    DiscountTaken = pa.DiscountTaken,
                    ApplicationDate = pa.ApplicationDate,
                    InvoiceTotalAmount = pa.VendorInvoice?.TotalAmount ?? 0,
                    InvoiceAmountPaid = pa.VendorInvoice?.AmountPaid ?? 0
                }).ToList()
            };
        }

        #endregion
    }
}
