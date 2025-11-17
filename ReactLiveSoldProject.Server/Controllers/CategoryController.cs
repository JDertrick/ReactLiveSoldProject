using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReactLiveSoldProject.Server.Controllers.Base;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;

namespace ReactLiveSoldProject.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : BaseController
    {
        private readonly ILogger<CategoryController> _logger;
        private readonly ICategoryService _categoryService;

        public CategoryController(ILogger<CategoryController> logger, ICategoryService categoryService)
        {
            _logger = logger;
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<ActionResult<IList<CategoryDto>>> GetCategories()
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                {
                    return Unauthorized();
                }

                var categories = await _categoryService.GetCategoriesAsync(organizationId.Value);

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando las categorias");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(Guid id)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                {
                    return Unauthorized();
                }

                var category = await _categoryService.GetCategoryAsync(organizationId.Value, id);

                if (category == null)
                {
                    return NotFound();
                }

                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando la categoria");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateCategory([FromBody] CreateCategoryDto categoryDto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                {
                    return Unauthorized();
                }

                await _categoryService.CreateCategoryAsync(organizationId.Value, categoryDto);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando la categoria");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto categoryDto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                {
                    return Unauthorized();
                }

                await _categoryService.UpdateCategoryAsync(organizationId.Value, id, categoryDto);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando la categoria");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                {
                    return Unauthorized();
                }

                await _categoryService.DeleteCategoryAsync(organizationId.Value, id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando la categoria");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
