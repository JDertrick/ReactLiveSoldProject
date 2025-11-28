using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReactLiveSoldProject.ServerBL.DTOs.Accounting;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using System;
using System.Collections.Generic; // Added for List
using System.Threading.Tasks;

namespace ReactLiveSoldProject.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountingController : ControllerBase
    {
        private readonly IAccountingService _accountingService;
        private readonly IOrganizationService _organizationService; // Already injected

        public AccountingController(IAccountingService accountingService, IOrganizationService organizationService)
        {
            _accountingService = accountingService;
            _organizationService = organizationService; // Already injected
        }

        // --- Chart of Accounts Endpoints ---

        [HttpGet("accounts")]
        public async Task<IActionResult> GetChartOfAccounts()
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var accounts = await _accountingService.GetChartOfAccountsAsync(organizationId.Value);
                return Ok(accounts);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Ocurrió un error interno al obtener el plan de cuentas.");
            }
        }

        [HttpPost("accounts")]
        public async Task<IActionResult> CreateChartOfAccount([FromBody] CreateChartOfAccountDto createDto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var account = await _accountingService.CreateChartOfAccountAsync(organizationId.Value, createDto);
                return CreatedAtAction(nameof(GetChartOfAccounts), new { id = account.Id }, account);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Ocurrió un error interno al crear la cuenta contable.");
            }
        }

        // --- Journal Entries Endpoints ---

        [HttpGet("journalentries")]
        public async Task<IActionResult> GetJournalEntries([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var journalEntries = await _accountingService.GetJournalEntriesAsync(organizationId.Value, fromDate, toDate);
                return Ok(journalEntries);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Ocurrió un error interno al obtener los asientos contables.");
            }
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
                return CreatedAtAction(nameof(GetJournalEntries), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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
