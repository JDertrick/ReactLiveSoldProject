using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.Server.Controllers.Base;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Models.Inventory;
using System.Security.Claims;

namespace ReactLiveSoldProject.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : BaseController
    {
        private readonly LiveSoldDbContext _context;
        private readonly IMapper _mapper;

        public CategoryController(LiveSoldDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
            {
                return Unauthorized();
            }

            var categories = await _context.Categories
                .Where(c => c.OrganizationId == organizationId)
                .Include(c => c.Children)
                .ToListAsync();

            return _mapper.Map<List<CategoryDto>>(categories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(Guid id)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
            {
                return Unauthorized();
            }

            var category = await _context.Categories
                .Include(c => c.Children)
                .FirstOrDefaultAsync(c => c.Id == id && c.OrganizationId == organizationId);

            if (category == null)
            {
                return NotFound();
            }

            return _mapper.Map<CategoryDto>(category);
        }

        [HttpPost]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CategoryDto categoryDto)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
            {
                return Unauthorized();
            }

            var category = _mapper.Map<Category>(categoryDto);
            category.OrganizationId = organizationId.Value;

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, _mapper.Map<CategoryDto>(category));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(Guid id, CategoryDto categoryDto)
        {
            if (id != categoryDto.Id)
            {
                return BadRequest();
            }

            var organizationId = GetOrganizationId();
            if (organizationId == null)
            {
                return Unauthorized();
            }

            var category = await _context.Categories.FindAsync(id);

            if (category == null || category.OrganizationId != organizationId)
            {
                return NotFound();
            }

            _mapper.Map(categoryDto, category);
            category.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
            {
                return Unauthorized();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null || category.OrganizationId != organizationId)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoryExists(Guid id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
