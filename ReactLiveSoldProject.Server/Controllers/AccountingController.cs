using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReactLiveSoldProject.ServerBL.DTOs.Accounting;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using System;
using System.Threading.Tasks;

namespace ReactLiveSoldProject.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountingController : ControllerBase
    {
        private readonly IAccountingService _accountingService;
        private readonly IOrganizationService _organizationService;

        public AccountingController(IAccountingService accountingService, IOrganizationService organizationService)
        {
            _accountingService = accountingService;
            _organizationService = organizationService;
        }

        // --- Chart of Accounts Endpoints (A implementar en un futuro) ---

        [HttpGet("accounts")]
        public async Task<IActionResult> GetChartOfAccounts()
        {
            return Ok("Endpoint para obtener Plan de Cuentas (pendiente de implementación)");
        }

        [HttpPost("accounts")]
        public async Task<IActionResult> CreateChartOfAccount([FromBody] CreateChartOfAccountDto createDto)
        {
            return Ok("Endpoint para crear una Cuenta Contable (pendiente de implementación)");
        }

        // --- Journal Entries Endpoints ---

        [HttpGet("journalentries")]
        public async Task<IActionResult> GetJournalEntries()
        {
            return Ok("Endpoint para obtener Asientos Contables (pendiente de implementación)");
        }

        [HttpPost("journalentries")]
        public async Task<IActionResult> CreateJournalEntry([FromBody] CreateJournalEntryDto createDto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var result = await _accountingService.CreateJournalEntryAsync(organizationId.Value, createDto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Ocurrió un error interno al crear el asiento contable.");
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
