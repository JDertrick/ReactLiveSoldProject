using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.Models.Authentication;

namespace ReactLiveSoldProject.ServerBL.Helpers
{
    public class DatabaseSeeder
    {
        private readonly LiveSoldDbContext _dbContext;

        public DatabaseSeeder(LiveSoldDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Crea datos iniciales si la base de datos está vacía
        /// - SuperAdmin user
        /// - Test organization
        /// - Organization member linking them
        /// </summary>
        public async Task SeedAsync()
        {
            // Verificar si ya existen datos
            var hasUsers = await _dbContext.Users.AnyAsync();
            if (hasUsers)
            {
                // Ya hay datos, no hacer nada
                return;
            }

            // 1. Crear SuperAdmin User
            var superAdminId = Guid.NewGuid();
            var superAdmin = new User
            {
                Id = superAdminId,
                FirstName = "Super",
                LastName = "Admin",
                Email = "admin@livesold.com",
                PasswordHash = PasswordHelper.HashPassword("Admin123!"),
                IsSuperAdmin = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Users.Add(superAdmin);

            // 2. Crear Organization de prueba
            var organizationId = Guid.NewGuid();
            var testOrganization = new Organization
            {
                Id = organizationId,
                Name = "Tienda Demo",
                Slug = "tienda-demo",
                LogoUrl = null,
                PrimaryContactEmail = "contacto@tiendademo.com",
                PlanType = PlanType.Standard,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Organizations.Add(testOrganization);

            // 3. Crear Owner User para la organización de prueba
            var ownerId = Guid.NewGuid();
            var owner = new User
            {
                Id = ownerId,
                FirstName = "Juan",
                LastName = "Pérez",
                Email = "juan@tiendademo.com",
                PasswordHash = PasswordHelper.HashPassword("Owner123!"),
                IsSuperAdmin = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Users.Add(owner);

            // 4. Vincular el Owner con la organización
            var orgMember = new OrganizationMember
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                UserId = ownerId,
                Role = UserRole.Owner,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.OrganizationMembers.Add(orgMember);

            // 5. Guardar todos los cambios
            await _dbContext.SaveChangesAsync();

            Console.WriteLine("✓ Datos semilla creados exitosamente:");
            Console.WriteLine($"  - SuperAdmin: admin@livesold.com / Admin123!");
            Console.WriteLine($"  - Owner: juan@tiendademo.com / Owner123!");
            Console.WriteLine($"  - Organización: Tienda Demo (slug: tienda-demo)");
        }
    }
}
