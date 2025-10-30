using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Helpers;
using ReactLiveSoldProject.ServerBL.Models.Authentication;

namespace ReactLiveSoldProject.ServerBL.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly LiveSoldDbContext _dbContext;

        public OrganizationService(LiveSoldDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<OrganizationDto>> GetAllOrganizationsAsync()
        {
            var organizations = await _dbContext.Organizations
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return organizations.Select(o => MapToDto(o)).ToList();
        }

        public async Task<OrganizationDto?> GetOrganizationByIdAsync(Guid id)
        {
            var organization = await _dbContext.Organizations
                .FirstOrDefaultAsync(o => o.Id == id);

            return organization != null ? MapToDto(organization) : null;
        }

        public async Task<OrganizationPublicDto?> GetOrganizationBySlugAsync(string slug)
        {
            var organization = await _dbContext.Organizations
                .FirstOrDefaultAsync(o => o.Slug == slug && o.IsActive);

            if (organization == null)
                return null;

            // IMPORTANTE: Solo devolver datos seguros
            return new OrganizationPublicDto
            {
                Name = organization.Name,
                LogoUrl = organization.LogoUrl
            };
        }

        public async Task<OrganizationDto> CreateOrganizationAsync(CreateOrganizationDto dto)
        {
            // Generar slug si no se proporcionó
            var slug = string.IsNullOrWhiteSpace(dto.Slug)
                ? SlugHelper.GenerateSlug(dto.Name)
                : dto.Slug;

            // Asegurar que el slug sea único
            slug = await SlugHelper.EnsureUniqueSlugAsync(_dbContext, slug);

            var organization = new Organization
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Slug = slug,
                LogoUrl = dto.LogoUrl,
                PrimaryContactEmail = dto.PrimaryContactEmail,
                PlanType = dto.PlanType,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Organizations.Add(organization);
            await _dbContext.SaveChangesAsync();

            return MapToDto(organization);
        }

        public async Task<OrganizationDto> UpdateOrganizationAsync(Guid id, CreateOrganizationDto dto)
        {
            var organization = await _dbContext.Organizations
                .FirstOrDefaultAsync(o => o.Id == id);

            if (organization == null)
                throw new KeyNotFoundException("Organización no encontrada");

            // Actualizar slug si cambió el nombre y no se proporcionó uno nuevo
            var newSlug = string.IsNullOrWhiteSpace(dto.Slug)
                ? SlugHelper.GenerateSlug(dto.Name)
                : dto.Slug;

            // Si el slug cambió, asegurar que sea único (excluyendo esta organización)
            if (newSlug != organization.Slug)
            {
                newSlug = await SlugHelper.EnsureUniqueSlugAsync(_dbContext, newSlug, id);
            }

            organization.Name = dto.Name;
            organization.Slug = newSlug;
            organization.LogoUrl = dto.LogoUrl;
            organization.PrimaryContactEmail = dto.PrimaryContactEmail;
            organization.PlanType = dto.PlanType;
            organization.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return MapToDto(organization);
        }

        public async Task DeleteOrganizationAsync(Guid id)
        {
            var organization = await _dbContext.Organizations
                .FirstOrDefaultAsync(o => o.Id == id);

            if (organization == null)
                throw new KeyNotFoundException("Organización no encontrada");

            // Verificar si tiene datos relacionados
            var hasMembers = await _dbContext.OrganizationMembers
                .AnyAsync(om => om.OrganizationId == id);

            var hasCustomers = await _dbContext.Customers
                .AnyAsync(c => c.OrganizationId == id);

            var hasProducts = await _dbContext.Products
                .AnyAsync(p => p.OrganizationId == id);

            if (hasMembers || hasCustomers || hasProducts)
            {
                throw new InvalidOperationException(
                    "No se puede eliminar la organización porque tiene datos relacionados. " +
                    "Considere desactivarla en lugar de eliminarla.");
            }

            _dbContext.Organizations.Remove(organization);
            await _dbContext.SaveChangesAsync();
        }

        private static OrganizationDto MapToDto(Organization organization)
        {
            return new OrganizationDto
            {
                Id = organization.Id,
                Name = organization.Name,
                Slug = organization.Slug,
                LogoUrl = organization.LogoUrl,
                PrimaryContactEmail = organization.PrimaryContactEmail,
                PlanType = organization.PlanType.ToString(),
                IsActive = organization.IsActive,
                CreatedAt = organization.CreatedAt
            };
        }
    }
}
