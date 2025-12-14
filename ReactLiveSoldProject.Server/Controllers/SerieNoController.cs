using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReactLiveSoldProject.Server.Controllers.Base;
using ReactLiveSoldProject.ServerBL.DTOs.Configuration;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Configuration;

namespace ReactLiveSoldProject.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SerieNoController : BaseController
    {
        private readonly ISerieNoService _serieNoService;

        public SerieNoController(ISerieNoService serieNoService)
        {
            _serieNoService = serieNoService;
        }

        #region NoSerie CRUD Endpoints

        /// <summary>
        /// Obtiene todas las series numéricas de la organización
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "Employee")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var organizationId = GetOrganizationId();
                var series = await _serieNoService.GetAllAsync(organizationId.Value);
                return Ok(series);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene una serie numérica por ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = "Employee")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var organizationId = GetOrganizationId();
                var serie = await _serieNoService.GetByIdAsync(organizationId.Value, id);

                if (serie == null)
                    return NotFound(new { message = "Serie numérica no encontrada" });

                return Ok(serie);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene una serie numérica por código
        /// </summary>
        [HttpGet("code/{code}")]
        [Authorize(Policy = "Employee")]
        public async Task<IActionResult> GetByCode(string code)
        {
            try
            {
                var organizationId = GetOrganizationId();
                var serie = await _serieNoService.GetByCodeAsync(organizationId.Value, code);

                if (serie == null)
                    return NotFound(new { message = $"Serie numérica '{code}' no encontrada" });

                return Ok(serie);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Crea una nueva serie numérica
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "OrgOwner")]
        public async Task<IActionResult> Create([FromBody] CreateNoSerieDto dto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                var serie = await _serieNoService.CreateAsync(organizationId.Value, dto);
                return CreatedAtAction(nameof(GetById), new { id = serie.Id }, serie);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear la serie numérica", detail = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza una serie numérica existente
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "OrgOwner")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateNoSerieDto dto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                var serie = await _serieNoService.UpdateAsync(organizationId.Value, id, dto);
                return Ok(serie);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar la serie numérica", detail = ex.Message });
            }
        }

        /// <summary>
        /// Elimina una serie numérica
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "OrgOwner")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var organizationId = GetOrganizationId();
                await _serieNoService.DeleteAsync(organizationId.Value, id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar la serie numérica", detail = ex.Message });
            }
        }

        #endregion

        #region NoSerieLine Endpoints

        /// <summary>
        /// Agrega una nueva línea a una serie numérica
        /// </summary>
        [HttpPost("{serieId}/lines")]
        [Authorize(Policy = "OrgOwner")]
        public async Task<IActionResult> AddLine(Guid serieId, [FromBody] CreateNoSerieLineDto dto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                var line = await _serieNoService.AddLineAsync(organizationId.Value, serieId, dto);
                return Ok(line);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al agregar la línea", detail = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza una línea de serie numérica
        /// </summary>
        [HttpPut("lines/{lineId}")]
        [Authorize(Policy = "OrgOwner")]
        public async Task<IActionResult> UpdateLine(Guid lineId, [FromBody] UpdateNoSerieLineDto dto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                var line = await _serieNoService.UpdateLineAsync(organizationId.Value, lineId, dto);
                return Ok(line);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar la línea", detail = ex.Message });
            }
        }

        /// <summary>
        /// Elimina una línea de serie numérica
        /// </summary>
        [HttpDelete("lines/{lineId}")]
        [Authorize(Policy = "OrgOwner")]
        public async Task<IActionResult> DeleteLine(Guid lineId)
        {
            try
            {
                var organizationId = GetOrganizationId();
                await _serieNoService.DeleteLineAsync(organizationId.Value, lineId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar la línea", detail = ex.Message });
            }
        }

        #endregion

        #region Number Generation Endpoints (Funcionalidad Principal)

        /// <summary>
        /// Obtiene el siguiente número disponible de una serie numérica
        /// </summary>
        /// <param name="code">Código de la serie numérica</param>
        /// <param name="date">Fecha para la cual se obtiene el número (opcional, por defecto hoy)</param>
        [HttpGet("next/{code}")]
        [Authorize(Policy = "Employee")]
        public async Task<IActionResult> GetNextNumber(string code, [FromQuery] DateTime? date = null)
        {
            try
            {
                var organizationId = GetOrganizationId();
                var nextNumber = await _serieNoService.GetNextNumberAsync(organizationId.Value, code, date);
                return Ok(new { serieCode = code, nextNumber, date = date ?? DateTime.UtcNow });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el siguiente número", detail = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el siguiente número disponible para un tipo de documento
        /// </summary>
        /// <param name="documentType">Tipo de documento (Customer, Vendor, Invoice, etc.)</param>
        /// <param name="date">Fecha para la cual se obtiene el número (opcional, por defecto hoy)</param>
        [HttpGet("next/type/{documentType}")]
        [Authorize(Policy = "Employee")]
        public async Task<IActionResult> GetNextNumberByType(DocumentType documentType, [FromQuery] DateTime? date = null)
        {
            try
            {
                var organizationId = GetOrganizationId();
                var nextNumber = await _serieNoService.GetNextNumberByTypeAsync(organizationId.Value, documentType, date);
                return Ok(new { documentType = documentType.ToString(), nextNumber, date = date ?? DateTime.UtcNow });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el siguiente número", detail = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene todas las series numéricas para un tipo de documento
        /// </summary>
        [HttpGet("type/{documentType}")]
        [Authorize(Policy = "Employee")]
        public async Task<IActionResult> GetByDocumentType(DocumentType documentType)
        {
            try
            {
                var organizationId = GetOrganizationId();
                var series = await _serieNoService.GetByDocumentTypeAsync(organizationId.Value, documentType);
                return Ok(series);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene la serie por defecto para un tipo de documento
        /// </summary>
        [HttpGet("type/{documentType}/default")]
        [Authorize(Policy = "Employee")]
        public async Task<IActionResult> GetDefaultSerieByType(DocumentType documentType)
        {
            try
            {
                var organizationId = GetOrganizationId();
                var serie = await _serieNoService.GetDefaultSerieByTypeAsync(organizationId.Value, documentType);

                if (serie == null)
                    return NotFound(new { message = $"No hay una serie por defecto para el tipo '{documentType}'" });

                return Ok(serie);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Validation Endpoints

        /// <summary>
        /// Valida si un número es válido para una serie numérica
        /// </summary>
        [HttpGet("validate/{code}/{number}")]
        [Authorize(Policy = "Employee")]
        public async Task<IActionResult> ValidateNumber(string code, string number)
        {
            try
            {
                var organizationId = GetOrganizationId();
                var isValid = await _serieNoService.ValidateNumberAsync(organizationId.Value, code, number);
                return Ok(new { serieCode = code, number, isValid });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Verifica si un número está disponible (no ha sido usado aún)
        /// </summary>
        [HttpGet("available/{code}/{number}")]
        [Authorize(Policy = "Employee")]
        public async Task<IActionResult> IsNumberAvailable(string code, string number)
        {
            try
            {
                var organizationId = GetOrganizationId();
                var isAvailable = await _serieNoService.IsNumberAvailableAsync(organizationId.Value, code, number);
                return Ok(new { serieCode = code, number, isAvailable });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion
    }
}
