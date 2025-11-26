using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Vendors;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    public class VendorService : IVendorService
    {
        private readonly LiveSoldDbContext _dbContext;
        private readonly IMapper _mapper;

        public VendorService(LiveSoldDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<VendorDto>> GetVendorsByOrganizationAsync(Guid organizationId, string? searchTerm = null, string? status = null)
        {
            var query = _dbContext.Vendors
                .Include(v => v.Contact)
                .Include(v => v.AssignedBuyer)
                .Where(v => v.OrganizationId == organizationId);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower().Trim();
                query = query.Where(v =>
                    (v.VendorCode != null && v.VendorCode.ToLower().Contains(lowerSearchTerm)) ||
                    (v.Contact.FirstName != null && v.Contact.FirstName.ToLower().Contains(lowerSearchTerm)) ||
                    (v.Contact.LastName != null && v.Contact.LastName.ToLower().Contains(lowerSearchTerm)) ||
                    (v.Contact.Email != null && v.Contact.Email.ToLower().Contains(lowerSearchTerm)) ||
                    (v.Contact.Company != null && v.Contact.Company.ToLower().Contains(lowerSearchTerm))
                );
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status) && status.ToLower() != "all")
            {
                var isActive = status.ToLower() == "active";
                query = query.Where(v => v.IsActive == isActive);
            }

            var vendors = await query
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

            return vendors.Select(v => MapToDto(v)).ToList();
        }

        public async Task<VendorDto?> GetVendorByIdAsync(Guid vendorId, Guid organizationId)
        {
            var vendor = await _dbContext.Vendors
                .Include(v => v.Contact)
                .Include(v => v.AssignedBuyer)
                .FirstOrDefaultAsync(v => v.Id == vendorId && v.OrganizationId == organizationId);

            return vendor != null ? MapToDto(vendor) : null;
        }

        public async Task<List<VendorDto>> SearchVendorsAsync(Guid organizationId, string searchTerm)
        {
            var normalizedSearch = searchTerm.ToLower().Trim();

            var vendors = await _dbContext.Vendors
                .Include(v => v.Contact)
                .Include(v => v.AssignedBuyer)
                .Where(v => v.OrganizationId == organizationId &&
                    (v.VendorCode != null && v.VendorCode.ToLower().Contains(normalizedSearch) ||
                     v.Contact.Email.ToLower().Contains(normalizedSearch) ||
                     v.Contact.FirstName != null && v.Contact.FirstName.ToLower().Contains(normalizedSearch) ||
                     v.Contact.LastName != null && v.Contact.LastName.ToLower().Contains(normalizedSearch) ||
                     v.Contact.Company != null && v.Contact.Company.ToLower().Contains(normalizedSearch)))
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

            return vendors.Select(v => MapToDto(v)).ToList();
        }

        public async Task<VendorDto> CreateVendorAsync(Guid organizationId, CreateVendorDto dto)
        {
            // Verificar que el contacto exista y pertenezca a la organización
            var contact = await _dbContext.Contacts
                .FirstOrDefaultAsync(c => c.Id == dto.ContactId && c.OrganizationId == organizationId);

            if (contact == null)
                throw new InvalidOperationException("El contacto no existe o no pertenece a esta organización");

            // Verificar que no exista un proveedor con el mismo contacto en esta organización
            var existingVendor = await _dbContext.Vendors
                .FirstOrDefaultAsync(v => v.OrganizationId == organizationId && v.ContactId == dto.ContactId);

            if (existingVendor != null)
                throw new InvalidOperationException("Ya existe un proveedor con este contacto en la organización");

            // Si tiene código de proveedor, verificar que sea único
            if (!string.IsNullOrWhiteSpace(dto.VendorCode))
            {
                var existingCode = await _dbContext.Vendors
                    .FirstOrDefaultAsync(v => v.OrganizationId == organizationId && v.VendorCode == dto.VendorCode);

                if (existingCode != null)
                    throw new InvalidOperationException("Ya existe un proveedor con este código en la organización");
            }

            // Verificar que el assigned buyer pertenezca a la organización (si se proporciona)
            if (dto.AssignedBuyerId.HasValue)
            {
                var buyerExists = await _dbContext.OrganizationMembers
                    .AnyAsync(om => om.OrganizationId == organizationId && om.UserId == dto.AssignedBuyerId.Value);

                if (!buyerExists)
                    throw new InvalidOperationException("El comprador asignado no pertenece a esta organización");
            }

            // Crear el proveedor
            var vendor = _mapper.Map<Vendor>(dto);
            vendor.Id = Guid.NewGuid();
            vendor.OrganizationId = organizationId;

            _dbContext.Vendors.Add(vendor);
            await _dbContext.SaveChangesAsync();

            // Recargar el proveedor con sus relaciones
            var createdVendor = await _dbContext.Vendors
                .Include(v => v.Contact)
                .Include(v => v.AssignedBuyer)
                .FirstAsync(v => v.Id == vendor.Id);

            return MapToDto(createdVendor);
        }

        public async Task<VendorDto> UpdateVendorAsync(Guid vendorId, Guid organizationId, UpdateVendorDto dto)
        {
            var vendor = await _dbContext.Vendors
                .Include(v => v.Contact)
                .Include(v => v.AssignedBuyer)
                .FirstOrDefaultAsync(v => v.Id == vendorId && v.OrganizationId == organizationId);

            if (vendor == null)
                throw new KeyNotFoundException("Proveedor no encontrado");

            // Verificar contacto único (excluyendo este proveedor)
            if (dto.ContactId.HasValue && dto.ContactId.Value != vendor.ContactId)
            {
                // Verificar que el contacto exista y pertenezca a la organización
                var contact = await _dbContext.Contacts
                    .FirstOrDefaultAsync(c => c.Id == dto.ContactId.Value && c.OrganizationId == organizationId);

                if (contact == null)
                    throw new InvalidOperationException("El contacto no existe o no pertenece a esta organización");

                var contactExists = await _dbContext.Vendors
                    .AnyAsync(v => v.OrganizationId == organizationId && v.ContactId == dto.ContactId.Value && v.Id != vendorId);

                if (contactExists)
                    throw new InvalidOperationException("Ya existe otro proveedor con este contacto en la organización");
            }

            // Verificar código único (excluyendo este proveedor)
            if (!string.IsNullOrWhiteSpace(dto.VendorCode) && dto.VendorCode != vendor.VendorCode)
            {
                var codeExists = await _dbContext.Vendors
                    .AnyAsync(v => v.OrganizationId == organizationId && v.VendorCode == dto.VendorCode && v.Id != vendorId);

                if (codeExists)
                    throw new InvalidOperationException("Ya existe otro proveedor con este código en la organización");
            }

            // Verificar que el assigned buyer pertenezca a la organización (si se proporciona)
            if (dto.AssignedBuyerId.HasValue)
            {
                var buyerExists = await _dbContext.OrganizationMembers
                    .AnyAsync(om => om.OrganizationId == organizationId && om.UserId == dto.AssignedBuyerId.Value);

                if (!buyerExists)
                    throw new InvalidOperationException("El comprador asignado no pertenece a esta organización");
            }

            // Actualizar campos
            if (dto.ContactId.HasValue)
                vendor.ContactId = dto.ContactId.Value;

            if (dto.AssignedBuyerId.HasValue)
                vendor.AssignedBuyerId = dto.AssignedBuyerId.Value;

            if (dto.VendorCode != null)
                vendor.VendorCode = dto.VendorCode;

            if (dto.Notes != null)
                vendor.Notes = dto.Notes;

            if (dto.PaymentTerms != null)
                vendor.PaymentTerms = dto.PaymentTerms;

            if (dto.CreditLimit.HasValue)
                vendor.CreditLimit = dto.CreditLimit.Value;

            if (dto.IsActive.HasValue)
                vendor.IsActive = dto.IsActive.Value;

            vendor.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return MapToDto(vendor);
        }

        public async Task DeleteVendorAsync(Guid vendorId, Guid organizationId)
        {
            var vendor = await _dbContext.Vendors
                .FirstOrDefaultAsync(v => v.Id == vendorId && v.OrganizationId == organizationId);

            if (vendor == null)
                throw new KeyNotFoundException("Proveedor no encontrado");

            // Aquí se podría verificar si tiene órdenes de compra cuando se implemente esa funcionalidad
            // var hasPurchaseOrders = await _dbContext.PurchaseOrders
            //     .AnyAsync(po => po.VendorId == vendorId);
            //
            // if (hasPurchaseOrders)
            // {
            //     throw new InvalidOperationException(
            //         "No se puede eliminar el proveedor porque tiene órdenes de compra asociadas. " +
            //         "Considere desactivarlo en lugar de eliminarlo.");
            // }

            _dbContext.Vendors.Remove(vendor);
            await _dbContext.SaveChangesAsync();
        }

        private static VendorDto MapToDto(Vendor vendor)
        {
            return new VendorDto
            {
                Id = vendor.Id,
                OrganizationId = vendor.OrganizationId,
                ContactId = vendor.ContactId,
                Contact = vendor.Contact != null ? new ContactDto
                {
                    Id = vendor.Contact.Id,
                    OrganizationId = vendor.Contact.OrganizationId,
                    FirstName = vendor.Contact.FirstName,
                    LastName = vendor.Contact.LastName,
                    Email = vendor.Contact.Email,
                    Phone = vendor.Contact.Phone,
                    Address = vendor.Contact.Address,
                    City = vendor.Contact.City,
                    State = vendor.Contact.State,
                    PostalCode = vendor.Contact.PostalCode,
                    Country = vendor.Contact.Country,
                    Company = vendor.Contact.Company,
                    JobTitle = vendor.Contact.JobTitle,
                    CreatedAt = vendor.Contact.CreatedAt,
                    UpdatedAt = vendor.Contact.UpdatedAt,
                    IsActive = vendor.Contact.IsActive
                } : null,
                AssignedBuyerId = vendor.AssignedBuyerId,
                AssignedBuyerName = vendor.AssignedBuyer != null
                    ? $"{vendor.AssignedBuyer.FirstName} {vendor.AssignedBuyer.LastName}".Trim()
                    : null,
                VendorCode = vendor.VendorCode,
                Notes = vendor.Notes,
                PaymentTerms = vendor.PaymentTerms,
                CreditLimit = vendor.CreditLimit,
                CreatedAt = vendor.CreatedAt,
                UpdatedAt = vendor.UpdatedAt,
                IsActive = vendor.IsActive
            };
        }
    }
}
