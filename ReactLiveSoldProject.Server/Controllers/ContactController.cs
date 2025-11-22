using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using System.Security.Claims;

namespace ReactLiveSoldProject.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Employee")]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;
        private readonly ILogger<ContactController> _logger;

        public ContactController(
            IContactService contactService,
            ILogger<ContactController> logger)
        {
            _contactService = contactService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los contactos de la organización del usuario autenticado
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<ContactDto>>> GetContacts([FromQuery] string? searchTerm, [FromQuery] string? status)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var contacts = await _contactService.GetContactsByOrganizationAsync(organizationId.Value);

                // Server-side filtering
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var lowerSearchTerm = searchTerm.ToLower();
                    contacts = contacts.Where(c =>
                        (c.FirstName != null && c.FirstName.ToLower().Contains(lowerSearchTerm)) ||
                        (c.LastName != null && c.LastName.ToLower().Contains(lowerSearchTerm)) ||
                        (c.Email != null && c.Email.ToLower().Contains(lowerSearchTerm)) ||
                        (c.Phone != null && c.Phone.ToLower().Contains(lowerSearchTerm)) ||
                        (c.Company != null && c.Company.ToLower().Contains(lowerSearchTerm))
                    ).ToList();
                }

                if (!string.IsNullOrEmpty(status) && status.ToLower() != "all")
                {
                    var isActive = status.ToLower() == "active";
                    contacts = contacts.Where(c => c.IsActive == isActive).ToList();
                }

                return Ok(contacts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contacts");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene un contacto por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ContactDto>> GetContact(Guid id)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var contact = await _contactService.GetContactByIdAsync(id, organizationId.Value);

                if (contact == null)
                    return NotFound(new { message = "Contacto no encontrado" });

                return Ok(contact);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contact {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Busca contactos por email, nombre, teléfono o empresa
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<List<ContactDto>>> SearchContacts([FromQuery] string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return BadRequest(new { message = "El término de búsqueda es requerido" });

                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var contacts = await _contactService.SearchContactsAsync(organizationId.Value, q);
                return Ok(contacts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching contacts with term: {SearchTerm}", q);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crea un nuevo contacto
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ContactDto>> CreateContact([FromBody] CreateContactDto dto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var contact = await _contactService.CreateContactAsync(organizationId.Value, dto);

                return CreatedAtAction(
                    nameof(GetContact),
                    new { id = contact.Id },
                    contact);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation creating contact: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contact");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza un contacto existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ContactDto>> UpdateContact(Guid id, [FromBody] UpdateContactDto dto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var contact = await _contactService.UpdateContactAsync(id, organizationId.Value, dto);
                return Ok(contact);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Contact not found: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation updating contact {Id}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contact {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Elimina un contacto (solo si no está asociado a clientes o proveedores)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "OrgOwner")]
        public async Task<ActionResult> DeleteContact(Guid id)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                await _contactService.DeleteContactAsync(id, organizationId.Value);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Contact not found: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot delete contact {Id}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contact {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        private Guid? GetOrganizationId()
        {
            var claim = User.FindFirst("OrganizationId");
            if (claim != null && Guid.TryParse(claim.Value, out var organizationId))
                return organizationId;
            return null;
        }
    }
}
