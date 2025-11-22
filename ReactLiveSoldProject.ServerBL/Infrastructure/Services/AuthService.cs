using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Helpers;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly LiveSoldDbContext _dbContext;
        private readonly JwtHelper _jwtHelper;

        public AuthService(LiveSoldDbContext dbContext, JwtHelper jwtHelper)
        {
            _dbContext = dbContext;
            _jwtHelper = jwtHelper;
        }

        public async Task<LoginResponseDto> EmployeeLoginAsync(LoginRequestDto request)
        {
            // Buscar usuario por email
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                throw new UnauthorizedAccessException("Email o contraseña incorrectos");

            // Verificar password
            if (!PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Email o contraseña incorrectos");

            // Si es SuperAdmin, no necesita OrganizationMember
            if (user.IsSuperAdmin)
            {
                var token = _jwtHelper.GenerateEmployeeToken(
                    user.Id,
                    user.Email,
                    "SuperAdmin",
                    null
                );

                return new LoginResponseDto
                {
                    Token = token,
                    User = new UserProfileDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Role = "SuperAdmin",
                        OrganizationId = null,
                        IsSuperAdmin = true
                    }
                };
            }

            // Buscar la organización del usuario
            var orgMember = await _dbContext.OrganizationMembers
                .FirstOrDefaultAsync(om => om.UserId == user.Id);

            if (orgMember == null)
                throw new UnauthorizedAccessException("Usuario no asociado a ninguna organización");

            // Generar token
            var employeeToken = _jwtHelper.GenerateEmployeeToken(
                user.Id,
                user.Email,
                orgMember.Role.ToString(),
                orgMember.OrganizationId
            );

            return new LoginResponseDto
            {
                Token = employeeToken,
                User = new UserProfileDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = orgMember.Role.ToString(),
                    OrganizationId = orgMember.OrganizationId,
                    IsSuperAdmin = false
                }
            };
        }

        public async Task<LoginResponseDto> CustomerPortalLoginAsync(CustomerPortalLoginRequestDto request)
        {
            // Buscar organización por slug
            var organization = await _dbContext.Organizations
                .FirstOrDefaultAsync(o => o.Slug == request.OrganizationSlug);

            if (organization == null)
                throw new UnauthorizedAccessException("Organización no encontrada");

            // Buscar cliente por email (ahora el email está en Contact)
            var customer = await _dbContext.Customers
                .Include(c => c.Contact)
                .FirstOrDefaultAsync(c => c.Contact.Email == request.Email);

            if (customer == null)
                throw new UnauthorizedAccessException("Email o contraseña incorrectos");

            // VALIDACIÓN CRÍTICA: Verificar que el customer pertenece a esta organización
            if (customer.OrganizationId != organization.Id)
                throw new UnauthorizedAccessException("Email o contraseña incorrectos");

            // Verificar password
            if (!PasswordHelper.VerifyPassword(request.Password, customer.PasswordHash))
                throw new UnauthorizedAccessException("Email o contraseña incorrectos");

            // Generar token de customer
            var token = _jwtHelper.GenerateCustomerToken(
                customer.Id,
                customer.Contact.Email,
                customer.OrganizationId
            );

            return new LoginResponseDto
            {
                Token = token,
                User = new UserProfileDto
                {
                    Id = customer.Id,
                    Email = customer.Contact.Email,
                    FirstName = customer.Contact.FirstName,
                    LastName = customer.Contact.LastName,
                    Role = "Customer",
                    OrganizationId = customer.OrganizationId,
                    OrganizationSlug = organization.Slug,
                    OrganizationName = organization.Name,
                    OrganizationLogoUrl = organization.LogoUrl,
                    IsSuperAdmin = false
                }
            };
        }

        public async Task<UserProfileDto> GetEmployeeProfileAsync(Guid userId)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new KeyNotFoundException("Usuario no encontrado");

            if (user.IsSuperAdmin)
            {
                return new UserProfileDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = "SuperAdmin",
                    OrganizationId = null,
                    IsSuperAdmin = true
                };
            }

            var orgMember = await _dbContext.OrganizationMembers
                .FirstOrDefaultAsync(om => om.UserId == userId);

            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = orgMember?.Role.ToString(),
                OrganizationId = orgMember?.OrganizationId,
                IsSuperAdmin = false
            };
        }

        public async Task<CustomerProfileDto> GetCustomerProfileAsync(Guid customerId)
        {
            var customer = await _dbContext.Customers
                .Include(c => c.Contact)
                .FirstOrDefaultAsync(c => c.Id == customerId);

            if (customer == null)
                throw new KeyNotFoundException("Cliente no encontrado");

            return new CustomerProfileDto
            {
                Id = customer.Id,
                Email = customer.Contact.Email,
                FirstName = customer.Contact.FirstName,
                LastName = customer.Contact.LastName,
                Phone = customer.Contact.Phone,
                OrganizationId = customer.OrganizationId
            };
        }
    }
}
