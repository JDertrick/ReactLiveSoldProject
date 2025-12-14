using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.Server.Controllers.Base;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Inventory;
using System.Security.Claims;

namespace ReactLiveSoldProject.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : BaseController
    {
        private readonly ILocationService _locationService;
        private readonly ILogger<LocationController> _logger;


        public LocationController(ILocationService locationService, ILogger<LocationController> logger)
        {
            _locationService = locationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IList<LocationDto>>> GetLocations()
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized();

                var locations = await _locationService.GetLocationsAsync(organizationId.Value);
                return Ok(locations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando las bodegas");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LocationDto>> GetLocation(Guid id)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                {
                    return Unauthorized();
                }

                var location = await _locationService.GetLocationAsync(organizationId.Value, id);

                if (location == null)
                {
                    return NotFound();
                }

                return Ok(location);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando la bodega");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<LocationDto>> CreateLocation(CreateLocationDto locationDto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                {
                    return Unauthorized();
                }

                await _locationService.CreateLocationAsync(organizationId.Value, locationDto);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando la bodega");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLocation(Guid id, UpdateLocationDto locationDto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                {
                    return Unauthorized();
                }

                await _locationService.UpdateLocationAsync(organizationId.Value, id, locationDto);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando la bodega");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(Guid id)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                {
                    return Unauthorized();
                }

                await _locationService.DeleteLocationAsync(organizationId.Value, id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando la bodega");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
