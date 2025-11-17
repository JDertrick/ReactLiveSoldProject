
using ReactLiveSoldProject.ServerBL.DTOs;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetCategoriesAsync(Guid organizationId);

        Task<CategoryDto> GetCategoryAsync(Guid organizationId, Guid id);

        Task CreateCategoryAsync(Guid organizationId, CreateCategoryDto categoryDto);

        Task UpdateCategoryAsync(Guid organizationId, Guid Id, UpdateCategoryDto categoryDto);

        Task DeleteCategoryAsync(Guid organizationId, Guid Id);
    }
}
