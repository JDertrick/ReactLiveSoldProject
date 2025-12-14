
using Mapster;
using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Inventory;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly LiveSoldDbContext _dbContext;

        public CategoryService(LiveSoldDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<CategoryDto>> GetCategoriesAsync(Guid organizationId)
        {
            try
            {
                var categories = await _dbContext.Categories
                    .Where(o => o.OrganizationId == organizationId)
                    .Include(c => c.Children)
                    .OrderByDescending(c => c.Name)
                    .ToListAsync();

                var categoriesDto = categories.Adapt<List<CategoryDto>>();

                return categoriesDto;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<CategoryDto> GetCategoryAsync(Guid organizationId, Guid id)
        {
            try
            {
                var category = await _dbContext.Categories
                    .Include(c => c.Children)
                    .FirstOrDefaultAsync(o => o.OrganizationId == organizationId && o.Id == id);

                var categoryDto = category.Adapt<CategoryDto>();

                return categoryDto;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task CreateCategoryAsync(Guid organizationId, CreateCategoryDto categoryDto)
        {
            try
            {
                var category = categoryDto.Adapt<Category>();
                category.OrganizationId = organizationId;

                _dbContext.Categories.Add(category);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateCategoryAsync(Guid organizationId, Guid Id, UpdateCategoryDto dto)
        {
            try
            {
                var category = await _dbContext.Categories.FindAsync(Id);

                if (category == null || category.OrganizationId != organizationId)
                {
                    throw new InvalidOperationException("No se puede actualizar una categoria que no existe o no pertenece a esta organización");
                }

                dto.Adapt(category);
                category.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task DeleteCategoryAsync(Guid organizationId, Guid Id)
        {
            try
            {
                var category = await _dbContext.Categories.FindAsync(Id);

                if (category == null || category.OrganizationId != organizationId)
                {
                    throw new InvalidOperationException("No se puede eliminar una categoria que no existe o no pertenece a esta organización");
                }

                _dbContext.Categories.Remove(category);

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
