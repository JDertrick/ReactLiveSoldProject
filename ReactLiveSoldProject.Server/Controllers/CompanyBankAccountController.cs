using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReactLiveSoldProject.Server.Controllers.Base;
using ReactLiveSoldProject.ServerBL.DTOs.Banking;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;

namespace ReactLiveSoldProject.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Employee")]
    public class CompanyBankAccountController : BaseController
    {
        private readonly ICompanyBankAccountService _companyBankAccountService;

        public CompanyBankAccountController(ICompanyBankAccountService companyBankAccountService)
        {
            _companyBankAccountService = companyBankAccountService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanyBankAccounts()
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                var accounts = await _companyBankAccountService.GetCompanyBankAccountsAsync(organizationId.Value);
                return Ok(accounts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener cuentas bancarias", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompanyBankAccount(Guid id)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                var account = await _companyBankAccountService.GetCompanyBankAccountByIdAsync(id, organizationId.Value);
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
        public async Task<IActionResult> CreateCompanyBankAccount([FromBody] CreateCompanyBankAccountDto dto)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                var account = await _companyBankAccountService.CreateCompanyBankAccountAsync(organizationId.Value, dto);
                return CreatedAtAction(
                    nameof(GetCompanyBankAccount),
                    new { id = account.Id },
                    account);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al crear cuenta bancaria", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompanyBankAccount(Guid id, [FromBody] UpdateCompanyBankAccountDto dto)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                var account = await _companyBankAccountService.UpdateCompanyBankAccountAsync(id, organizationId.Value, dto);
                return Ok(account);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al actualizar cuenta bancaria", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompanyBankAccount(Guid id)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                await _companyBankAccountService.DeleteCompanyBankAccountAsync(id, organizationId.Value);
                return Ok(new { message = "Cuenta bancaria eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al eliminar cuenta bancaria", error = ex.Message });
            }
        }
    }
}
