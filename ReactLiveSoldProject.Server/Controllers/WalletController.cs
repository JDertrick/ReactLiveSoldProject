using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Services;
using System.Security.Claims;

namespace ReactLiveSoldProject.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Employee")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly ILogger<WalletController> _logger;

        public WalletController(
            IWalletService walletService,
            ILogger<WalletController> logger)
        {
            _walletService = walletService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene el wallet de un cliente
        /// </summary>
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<WalletDto>> GetWalletByCustomer(Guid customerId)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var wallet = await _walletService.GetWalletByCustomerIdAsync(customerId, organizationId.Value);

                if (wallet == null)
                    return NotFound(new { message = "Wallet no encontrado" });

                return Ok(wallet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wallet for customer {CustomerId}", customerId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene todas las transacciones de un cliente
        /// </summary>
        [HttpGet("customer/{customerId}/transactions")]
        public async Task<ActionResult<List<WalletTransactionDto>>> GetTransactionsByCustomer(Guid customerId)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var transactions = await _walletService.GetTransactionsByCustomerIdAsync(customerId, organizationId.Value);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions for customer {CustomerId}", customerId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crea un depósito en la wallet del cliente
        /// </summary>
        [HttpPost("deposit")]
        public async Task<ActionResult<WalletTransactionDto>> CreateDeposit([FromBody] CreateWalletTransactionDto dto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized(new { message = "UserId no encontrado en el token" });

                var transaction = await _walletService.CreateDepositAsync(organizationId.Value, userId.Value, dto);
                return Ok(transaction);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Entity not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation creating deposit: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized deposit attempt: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating deposit");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crea un retiro de la wallet del cliente
        /// </summary>
        [HttpPost("withdrawal")]
        public async Task<ActionResult<WalletTransactionDto>> CreateWithdrawal([FromBody] CreateWalletTransactionDto dto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized(new { message = "UserId no encontrado en el token" });

                var transaction = await _walletService.CreateWithdrawalAsync(organizationId.Value, userId.Value, dto);
                return Ok(transaction);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Entity not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation creating withdrawal: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized withdrawal attempt: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating withdrawal");
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

        private Guid? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && Guid.TryParse(claim.Value, out var userId))
                return userId;
            return null;
        }
    }
}
