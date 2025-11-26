using Microsoft.EntityFrameworkCore;
using AutoMapper;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs.Purchases;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Purchases;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    /// <summary>
    /// 🤝 ProductVendorService - Gestiona la relación entre productos y proveedores
    /// Permite vincular productos con sus proveedores preferidos, precios de costo, tiempos de entrega, etc.
    /// </summary>
    public class ProductVendorService : IProductVendorService
    {
        private readonly LiveSoldDbContext _context;
        private readonly IMapper _mapper;

        public ProductVendorService(LiveSoldDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtiene lista de relaciones producto-proveedor con filtros opcionales
        /// </summary>
        public async Task<List<ProductVendorDto>> GetProductVendorsAsync(
            Guid organizationId,
            Guid? productId = null,
            Guid? vendorId = null)
        {
            var query = _context.ProductVendors
                .Include(pv => pv.Product)
                .Include(pv => pv.Vendor)
                    .ThenInclude(v => v.Contact)
                .Where(pv => pv.Product.OrganizationId == organizationId);

            // Filtrar por producto
            if (productId.HasValue)
            {
                query = query.Where(pv => pv.ProductId == productId.Value);
            }

            // Filtrar por proveedor
            if (vendorId.HasValue)
            {
                query = query.Where(pv => pv.VendorId == vendorId.Value);
            }

            var productVendors = await query
                .OrderBy(pv => pv.Product.Name)
                .ThenBy(pv => pv.Vendor.Contact.Company ?? pv.Vendor.Contact.FirstName)
                .ToListAsync();

            return _mapper.Map<List<ProductVendorDto>>(productVendors);
        }

        /// <summary>
        /// Obtiene una relación producto-proveedor por ID
        /// </summary>
        public async Task<ProductVendorDto?> GetProductVendorByIdAsync(Guid productVendorId, Guid organizationId)
        {
            var productVendor = await _context.ProductVendors
                .Include(pv => pv.Product)
                .Include(pv => pv.Vendor)
                    .ThenInclude(v => v.Contact)
                .FirstOrDefaultAsync(pv => pv.Id == productVendorId && pv.Product.OrganizationId == organizationId);

            return _mapper.Map<ProductVendorDto>(productVendor);
        }

        /// <summary>
        /// Crea una nueva relación producto-proveedor
        /// </summary>
        public async Task<ProductVendorDto> CreateProductVendorAsync(
            Guid organizationId,
            CreateProductVendorDto dto)
        {
            // Validar que el producto existe y pertenece a la organización
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId && p.OrganizationId == organizationId);

            if (product == null)
                throw new Exception($"Producto con ID {dto.ProductId} no encontrado");

            // Validar que el proveedor existe y pertenece a la organización
            var vendor = await _context.Vendors
                .FirstOrDefaultAsync(v => v.Id == dto.VendorId && v.OrganizationId == organizationId);

            if (vendor == null)
                throw new Exception($"Proveedor con ID {dto.VendorId} no encontrado");

            // Validar que no exista ya la relación
            var exists = await _context.ProductVendors
                .AnyAsync(pv => pv.ProductId == dto.ProductId && pv.VendorId == dto.VendorId);

            if (exists)
                throw new Exception("Ya existe una relación entre este producto y proveedor");

            // Si se marca como preferido, desmarcar otros preferidos para este producto
            if (dto.IsPreferred)
            {
                var otherPreferred = await _context.ProductVendors
                    .Where(pv => pv.ProductId == dto.ProductId && pv.IsPreferred)
                    .ToListAsync();

                foreach (var pv in otherPreferred)
                {
                    pv.IsPreferred = false;
                    pv.UpdatedAt = DateTime.UtcNow;
                }
            }

            var productVendor = new ProductVendor
            {
                Id = Guid.NewGuid(),
                ProductId = dto.ProductId,
                VendorId = dto.VendorId,
                VendorSKU = dto.VendorSKU,
                CostPrice = dto.CostPrice,
                LeadTimeDays = dto.LeadTimeDays,
                MinimumOrderQuantity = dto.MinimumOrderQuantity,
                IsPreferred = dto.IsPreferred,
                ValidFrom = dto.ValidFrom,
                ValidTo = dto.ValidTo,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ProductVendors.Add(productVendor);
            await _context.SaveChangesAsync();

            return await GetProductVendorByIdAsync(productVendor.Id, organizationId)
                ?? throw new Exception("Error al recuperar la relación producto-proveedor creada");
        }

        /// <summary>
        /// Actualiza una relación producto-proveedor
        /// </summary>
        public async Task<ProductVendorDto> UpdateProductVendorAsync(
            Guid productVendorId,
            Guid organizationId,
            UpdateProductVendorDto dto)
        {
            var productVendor = await _context.ProductVendors
                .Include(pv => pv.Product)
                .FirstOrDefaultAsync(pv => pv.Id == productVendorId && pv.Product.OrganizationId == organizationId);

            if (productVendor == null)
                throw new Exception($"Relación producto-proveedor con ID {productVendorId} no encontrada");

            // Si se marca como preferido, desmarcar otros preferidos para este producto
            if (dto.IsPreferred.HasValue && dto.IsPreferred.Value && !productVendor.IsPreferred)
            {
                var otherPreferred = await _context.ProductVendors
                    .Where(pv => pv.ProductId == productVendor.ProductId && pv.IsPreferred && pv.Id != productVendorId)
                    .ToListAsync();

                foreach (var pv in otherPreferred)
                {
                    pv.IsPreferred = false;
                    pv.UpdatedAt = DateTime.UtcNow;
                }
            }

            if (dto.VendorSKU != null)
                productVendor.VendorSKU = dto.VendorSKU;
            if (dto.CostPrice.HasValue)
                productVendor.CostPrice = dto.CostPrice.Value;
            if (dto.LeadTimeDays.HasValue)
                productVendor.LeadTimeDays = dto.LeadTimeDays.Value;
            if (dto.MinimumOrderQuantity.HasValue)
                productVendor.MinimumOrderQuantity = dto.MinimumOrderQuantity.Value;
            if (dto.IsPreferred.HasValue)
                productVendor.IsPreferred = dto.IsPreferred.Value;
            if (dto.ValidFrom.HasValue)
                productVendor.ValidFrom = dto.ValidFrom.Value;
            if (dto.ValidTo.HasValue)
                productVendor.ValidTo = dto.ValidTo.Value;
            if (dto.IsActive.HasValue)
                productVendor.IsActive = dto.IsActive.Value;

            productVendor.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetProductVendorByIdAsync(productVendor.Id, organizationId)
                ?? throw new Exception("Error al recuperar la relación producto-proveedor actualizada");
        }

        /// <summary>
        /// Elimina una relación producto-proveedor
        /// </summary>
        public async Task DeleteProductVendorAsync(Guid productVendorId, Guid organizationId)
        {
            var productVendor = await _context.ProductVendors
                .Include(pv => pv.Product)
                .FirstOrDefaultAsync(pv => pv.Id == productVendorId && pv.Product.OrganizationId == organizationId);

            if (productVendor == null)
                throw new Exception($"Relación producto-proveedor con ID {productVendorId} no encontrada");

            _context.ProductVendors.Remove(productVendor);
            await _context.SaveChangesAsync();
        }
    }
}
