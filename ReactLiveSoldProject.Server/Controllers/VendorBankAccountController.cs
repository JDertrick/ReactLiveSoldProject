using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReactLiveSoldProject.Server.Controllers.Base;
using ReactLiveSoldProject.ServerBL.DTOs.Vendors;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;

namespace ReactLiveSoldProject.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Employee")]
    public class VendorBankAccountController : BaseController
    {
        private readonly IVendorBankAccountService _vendorBankAccountService;

        public VendorBankAccountController(IVendorBankAccountService vendorBankAccountService)
        {
            _vendorBankAccountService = vendorBankAccountService;
        }

        [HttpGet]
        public async Task<IActionResult> GetVendorBankAccounts([FromQuery] Guid vendorId)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                var accounts = await _vendorBankAccountService.GetVendorBankAccountsAsync(organizationId.Value, vendorId);
                return Ok(accounts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener cuentas bancarias", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVendorBankAccount(Guid id)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                var account = await _vendorBankAccountService.GetVendorBankAccountByIdAsync(id, organizationId.Value);
                if (account == null)
                    return NotFound(new { message = "Cuenta bancaria no encontrada" });

                return Ok(account);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener cuenta bancaria", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateVendorBankAccount([FromBody] CreateVendorBankAccountDto dto)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                var account = await _vendorBankAccountService.CreateVendorBankAccountAsync(organizationId.Value, dto);
                return CreatedAtAction(
                    nameof(GetVendorBankAccount),
                    new { id = account.Id },
                    account);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al crear cuenta bancaria", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVendorBankAccount(Guid id, [FromBody] UpdateVendorBankAccountDto dto)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                var account = await _vendorBankAccountService.UpdateVendorBankAccountAsync(id, organizationId.Value, dto);
                return Ok(account);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al actualizar cuenta bancaria", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVendorBankAccount(Guid id)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                await _vendorBankAccountService.DeleteVendorBankAccountAsync(id, organizationId.Value);
                return Ok(new { message = "Cuenta bancaria eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al eliminar cuenta bancaria", error = ex.Message });
            }
        }
    }
}
