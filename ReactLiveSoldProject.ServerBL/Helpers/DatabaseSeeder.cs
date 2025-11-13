using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.Models.Authentication;
using ReactLiveSoldProject.ServerBL.Models.CustomerWallet;

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

            // 5. Crear Customer de prueba para el portal
            var customerId = Guid.NewGuid();
            var testCustomer = new Customer
            {
                Id = customerId,
                OrganizationId = organizationId,
                FirstName = "María",
                LastName = "García",
                Email = "maria@cliente.com",
                Phone = "+34612345678",
                PasswordHash = PasswordHelper.HashPassword("Customer123!"),
                AssignedSellerId = ownerId,
                Notes = "Cliente de prueba para el portal",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Customers.Add(testCustomer);

            // 6. Crear Wallet para el Customer de prueba
            var walletId = Guid.NewGuid();
            var testWallet = new Wallet
            {
                Id = walletId,
                OrganizationId = organizationId,
                CustomerId = customerId,
                Balance = 1000.00m, // Balance inicial de 1000
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Wallets.Add(testWallet);

            // 7. Guardar todos los cambios
            await _dbContext.SaveChangesAsync();

            Console.WriteLine("✓ Datos semilla creados exitosamente:");
            Console.WriteLine($"  - SuperAdmin: admin@livesold.com / Admin123!");
            Console.WriteLine($"  - Owner: juan@tiendademo.com / Owner123!");
            Console.WriteLine($"  - Customer: maria@cliente.com / Customer123!");
            Console.WriteLine($"  - Organización: Tienda Demo (slug: tienda-demo)");
            Console.WriteLine($"  - Wallet inicial: $1,000.00");
        }
    }
}
