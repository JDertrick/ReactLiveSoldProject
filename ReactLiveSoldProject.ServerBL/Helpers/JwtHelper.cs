using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ReactLiveSoldProject.ServerBL.Helpers
{
    public class JwtHelper
    {
        private readonly IConfiguration _configuration;

        public JwtHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Genera un token JWT para un empleado (User)
        /// </summary>
        public string GenerateEmployeeToken(
            Guid userId,
            string email,
            string role,
            Guid? organizationId = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Solo agregar OrganizationId si no es SuperAdmin
            if (organizationId.HasValue)
            {
                claims.Add(new Claim("OrganizationId", organizationId.Value.ToString()));
            }

            return GenerateToken(claims);
        }

        /// <summary>
        /// Genera un token JWT para un cliente (Customer)
        /// </summary>
        public string GenerateCustomerToken(
            Guid customerId,
            string email,
            Guid organizationId)
        {
            var claims = new List<Claim>
            {
                new Claim("CustomerId", customerId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim("OrganizationId", organizationId.ToString()),
                new Claim(ClaimTypes.Role, "Customer"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            return GenerateToken(claims);
        }

        /// <summary>
        /// Genera el token JWT con los claims proporcionados
        /// </summary>
        private string GenerateToken(List<Claim> claims)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresInMinutes = int.Parse(_configuration["Jwt:ExpiresInMinutes"]!);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
