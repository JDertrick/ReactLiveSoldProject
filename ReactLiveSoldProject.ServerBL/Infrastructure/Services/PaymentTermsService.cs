using Microsoft.EntityFrameworkCore;
using Mapster;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs.Purchases;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Purchases;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    /// <summary>
    /// 📅 PaymentTermsService - Gestiona términos de pago
    /// Ejemplos: "Net 30", "2/10 Net 30" (2% descuento si paga en 10 días, sino 30 días)
    /// </summary>
    public class PaymentTermsService : IPaymentTermsService
    {
        private readonly LiveSoldDbContext _context;

        public PaymentTermsService(LiveSoldDbContext context)
        {
            _context = context;
        }

        public async Task<List<PaymentTermsDto>> GetPaymentTermsAsync(Guid organizationId)
        {
            var paymentTerms = await _context.PaymentTerms
                .Where(pt => pt.OrganizationId == organizationId && pt.IsActive)
                .OrderBy(pt => pt.Description)
                .ToListAsync();

            return paymentTerms.Adapt<List<PaymentTermsDto>>();
        }

        public async Task<PaymentTermsDto?> GetPaymentTermsByIdAsync(Guid paymentTermsId, Guid organizationId)
        {
            var paymentTerms = await _context.PaymentTerms
                .FirstOrDefaultAsync(pt => pt.Id == paymentTermsId && pt.OrganizationId == organizationId);

            return paymentTerms.Adapt<PaymentTermsDto>();
        }

        public async Task<PaymentTermsDto> CreatePaymentTermsAsync(
            Guid organizationId,
            CreatePaymentTermsDto dto)
        {
            var paymentTerms = new PaymentTerms
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                Description = dto.Description,
                DueDays = dto.DueDays,
                DiscountPercentage = dto.DiscountPercentage,
                DiscountDays = dto.DiscountDays,
                IsActive = true
            };

            _context.PaymentTerms.Add(paymentTerms);
            await _context.SaveChangesAsync();

            return paymentTerms.Adapt<PaymentTermsDto>();
        }

        public async Task<PaymentTermsDto> UpdatePaymentTermsAsync(
            Guid paymentTermsId,
            Guid organizationId,
            UpdatePaymentTermsDto dto)
        {
            var paymentTerms = await _context.PaymentTerms
                .FirstOrDefaultAsync(pt => pt.Id == paymentTermsId && pt.OrganizationId == organizationId);

            if (paymentTerms == null)
                throw new Exception($"Términos de pago con ID {paymentTermsId} no encontrados");

            if (dto.Description != null)
                paymentTerms.Description = dto.Description;
            if (dto.DueDays.HasValue)
                paymentTerms.DueDays = dto.DueDays.Value;
            if (dto.DiscountPercentage.HasValue)
                paymentTerms.DiscountPercentage = dto.DiscountPercentage.Value;
            if (dto.DiscountDays.HasValue)
                paymentTerms.DiscountDays = dto.DiscountDays.Value;
            if (dto.IsActive.HasValue)
                paymentTerms.IsActive = dto.IsActive.Value;

            await _context.SaveChangesAsync();

            return paymentTerms.Adapt<PaymentTermsDto>();
        }

        public async Task DeletePaymentTermsAsync(Guid paymentTermsId, Guid organizationId)
        {
            var paymentTerms = await _context.PaymentTerms
                .FirstOrDefaultAsync(pt => pt.Id == paymentTermsId && pt.OrganizationId == organizationId);

            if (paymentTerms == null)
                throw new Exception($"Términos de pago con ID {paymentTermsId} no encontrados");

            // Soft delete
            paymentTerms.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
}
