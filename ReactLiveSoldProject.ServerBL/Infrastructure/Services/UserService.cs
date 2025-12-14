using Mapster;
using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Helpers;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Authentication;
using System.Collections.Generic;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly LiveSoldDbContext _dbContext;

        public UserService(LiveSoldDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ICollection<UserProfileDto>> GetUserAsync(Guid organizationId)
        {
            // Mapear desde OrganizationMember para obtener el Role
            var users = _dbContext.OrganizationMembers
                .Where(om => om.OrganizationId == organizationId)
                .ProjectToType<UserProfileDto>();

            return await users.ToArrayAsync();
        }

        public async Task<UserProfileDto> CreateUserAsync(CreateUserDto dto)
        {
            // Validar que la organización existe
            var organization = await _dbContext.Organizations
                .FirstOrDefaultAsync(o => o.Id == dto.OrganizationId);

            if (organization == null)
                throw new InvalidOperationException("Organización no encontrada");

            // Validar que el email no esté en uso
            var existingUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (existingUser != null)
                throw new InvalidOperationException("El email ya está en uso");

            // Crear el usuario
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PasswordHash = PasswordHelper.HashPassword(dto.Password),
                IsSuperAdmin = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);

            // Crear la relación con la organización
            var organizationMember = new OrganizationMember
            {
                Id = Guid.NewGuid(),
                OrganizationId = dto.OrganizationId,
                UserId = user.Id,
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.OrganizationMembers.Add(organizationMember);

            await _dbContext.SaveChangesAsync();

            // Retornar el usuario creado con su información completa
            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = dto.Role.ToString(),
                OrganizationId = dto.OrganizationId,
                IsSuperAdmin = false
            };
        }
    }
}
