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
    public class LocationController : BaseController
    {
        private readonly LiveSoldDbContext _context;
        private readonly IMapper _mapper;

        public LocationController(LiveSoldDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocationDto>>> GetLocations()
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
            {
                return Unauthorized();
            }

            var locations = await _context.Locations
                .Where(l => l.OrganizationId == organizationId)
                .ToListAsync();

            return _mapper.Map<List<LocationDto>>(locations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LocationDto>> GetLocation(Guid id)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
            {
                return Unauthorized();
            }

            var location = await _context.Locations
                .FirstOrDefaultAsync(l => l.Id == id && l.OrganizationId == organizationId);

            if (location == null)
            {
                return NotFound();
            }

            return _mapper.Map<LocationDto>(location);
        }

        [HttpPost]
        public async Task<ActionResult<LocationDto>> CreateLocation(LocationDto locationDto)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
            {
                return Unauthorized();
            }

            var location = _mapper.Map<Location>(locationDto);
            location.OrganizationId = organizationId.Value;

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLocation), new { id = location.Id }, _mapper.Map<LocationDto>(location));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLocation(Guid id, LocationDto locationDto)
        {
            if (id != locationDto.Id)
            {
                return BadRequest();
            }

            var organizationId = GetOrganizationId();
            if (organizationId == null)
            {
                return Unauthorized();
            }

            var location = await _context.Locations.FindAsync(id);

            if (location == null || location.OrganizationId != organizationId)
            {
                return NotFound();
            }

            _mapper.Map(locationDto, location);
            location.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LocationExists(id))
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
        public async Task<IActionResult> DeleteLocation(Guid id)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
            {
                return Unauthorized();
            }

            var location = await _context.Locations.FindAsync(id);
            if (location == null || location.OrganizationId != organizationId)
            {
                return NotFound();
            }

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LocationExists(Guid id)
        {
            return _context.Locations.Any(e => e.Id == id);
        }
    }
}
