using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Helpers;
using ReactLiveSoldProject.ServerBL.Models.CustomerWallet;

namespace ReactLiveSoldProject.ServerBL.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly LiveSoldDbContext _dbContext;

        public CustomerService(LiveSoldDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<CustomerDto>> GetCustomersByOrganizationAsync(Guid organizationId)
        {
            var customers = await _dbContext.Customers
                .Include(c => c.Wallet)
                .Include(c => c.AssignedSeller)
                .Where(c => c.OrganizationId == organizationId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return customers.Select(c => MapToDto(c)).ToList();
        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(Guid customerId, Guid organizationId)
        {
            var customer = await _dbContext.Customers
                .Include(c => c.Wallet)
                .Include(c => c.AssignedSeller)
                .FirstOrDefaultAsync(c => c.Id == customerId && c.OrganizationId == organizationId);

            return customer != null ? MapToDto(customer) : null;
        }

        public async Task<List<CustomerDto>> SearchCustomersAsync(Guid organizationId, string searchTerm)
        {
            var normalizedSearch = searchTerm.ToLower().Trim();

            var customers = await _dbContext.Customers
                .Include(c => c.Wallet)
                .Include(c => c.AssignedSeller)
                .Where(c => c.OrganizationId == organizationId &&
                    (c.Email.ToLower().Contains(normalizedSearch) ||
                     (c.FirstName != null && c.FirstName.ToLower().Contains(normalizedSearch)) ||
                     (c.LastName != null && c.LastName.ToLower().Contains(normalizedSearch)) ||
                     (c.Phone != null && c.Phone.Contains(normalizedSearch))))
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return customers.Select(c => MapToDto(c)).ToList();
        }

        public async Task<CustomerDto> CreateCustomerAsync(Guid organizationId, CreateCustomerDto dto)
        {
            // Verificar que no exista un cliente con el mismo email en esta organización
            var existingCustomer = await _dbContext.Customers
                .FirstOrDefaultAsync(c => c.OrganizationId == organizationId && c.Email == dto.Email);

            if (existingCustomer != null)
                throw new InvalidOperationException("Ya existe un cliente con este email en la organización");

            // Si tiene teléfono, verificar que tampoco exista
            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                var existingPhone = await _dbContext.Customers
                    .FirstOrDefaultAsync(c => c.OrganizationId == organizationId && c.Phone == dto.Phone);

                if (existingPhone != null)
                    throw new InvalidOperationException("Ya existe un cliente con este teléfono en la organización");
            }

            // Verificar que el assigned seller pertenezca a la organización (si se proporciona)
            if (dto.AssignedSellerId.HasValue)
            {
                var sellerExists = await _dbContext.OrganizationMembers
                    .AnyAsync(om => om.OrganizationId == organizationId && om.UserId == dto.AssignedSellerId.Value);

                if (!sellerExists)
                    throw new InvalidOperationException("El vendedor asignado no pertenece a esta organización");
            }

            // Crear el cliente
            var customerId = Guid.NewGuid();
            var customer = new Customer
            {
                Id = customerId,
                OrganizationId = organizationId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                PasswordHash = PasswordHelper.HashPassword(dto.Password),
                AssignedSellerId = dto.AssignedSellerId,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Customers.Add(customer);

            // Crear automáticamente el wallet para el cliente
            var wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                CustomerId = customerId,
                Balance = 0.00m,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Wallets.Add(wallet);

            await _dbContext.SaveChangesAsync();

            // Recargar el customer con sus relaciones
            var createdCustomer = await _dbContext.Customers
                .Include(c => c.Wallet)
                .Include(c => c.AssignedSeller)
                .FirstAsync(c => c.Id == customerId);

            return MapToDto(createdCustomer);
        }

        public async Task<CustomerDto> UpdateCustomerAsync(Guid customerId, Guid organizationId, UpdateCustomerDto dto)
        {
            var customer = await _dbContext.Customers
                .Include(c => c.Wallet)
                .Include(c => c.AssignedSeller)
                .FirstOrDefaultAsync(c => c.Id == customerId && c.OrganizationId == organizationId);

            if (customer == null)
                throw new KeyNotFoundException("Cliente no encontrado");

            // Verificar email único (excluyendo este cliente)
            if (dto.Email != customer.Email)
            {
                var emailExists = await _dbContext.Customers
                    .AnyAsync(c => c.OrganizationId == organizationId && c.Email == dto.Email && c.Id != customerId);

                if (emailExists)
                    throw new InvalidOperationException("Ya existe otro cliente con este email en la organización");
            }

            // Verificar teléfono único (excluyendo este cliente)
            if (!string.IsNullOrWhiteSpace(dto.Phone) && dto.Phone != customer.Phone)
            {
                var phoneExists = await _dbContext.Customers
                    .AnyAsync(c => c.OrganizationId == organizationId && c.Phone == dto.Phone && c.Id != customerId);

                if (phoneExists)
                    throw new InvalidOperationException("Ya existe otro cliente con este teléfono en la organización");
            }

            // Verificar que el assigned seller pertenezca a la organización (si se proporciona)
            if (dto.AssignedSellerId.HasValue)
            {
                var sellerExists = await _dbContext.OrganizationMembers
                    .AnyAsync(om => om.OrganizationId == organizationId && om.UserId == dto.AssignedSellerId.Value);

                if (!sellerExists)
                    throw new InvalidOperationException("El vendedor asignado no pertenece a esta organización");
            }

            // Actualizar campos
            customer.FirstName = dto.FirstName;
            customer.LastName = dto.LastName;
            customer.Email = dto.Email;
            customer.Phone = dto.Phone;
            customer.AssignedSellerId = dto.AssignedSellerId;
            customer.Notes = dto.Notes;
            customer.UpdatedAt = DateTime.UtcNow;

            // Actualizar contraseña solo si se proporciona una nueva
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                customer.PasswordHash = PasswordHelper.HashPassword(dto.Password);
            }

            await _dbContext.SaveChangesAsync();

            return MapToDto(customer);
        }

        public async Task DeleteCustomerAsync(Guid customerId, Guid organizationId)
        {
            var customer = await _dbContext.Customers
                .FirstOrDefaultAsync(c => c.Id == customerId && c.OrganizationId == organizationId);

            if (customer == null)
                throw new KeyNotFoundException("Cliente no encontrado");

            // Verificar si tiene órdenes
            var hasOrders = await _dbContext.SalesOrders
                .AnyAsync(so => so.CustomerId == customerId);

            if (hasOrders)
            {
                throw new InvalidOperationException(
                    "No se puede eliminar el cliente porque tiene órdenes de venta asociadas. " +
                    "Considere desactivarlo en lugar de eliminarlo.");
            }

            // Si llega aquí, se puede eliminar (el wallet se eliminará en cascada)
            _dbContext.Customers.Remove(customer);
            await _dbContext.SaveChangesAsync();
        }

        private static CustomerDto MapToDto(Customer customer)
        {
            return new CustomerDto
            {
                Id = customer.Id,
                OrganizationId = customer.OrganizationId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Phone = customer.Phone,
                AssignedSellerId = customer.AssignedSellerId,
                AssignedSellerName = customer.AssignedSeller != null
                    ? $"{customer.AssignedSeller.FirstName} {customer.AssignedSeller.LastName}".Trim()
                    : null,
                Notes = customer.Notes,
                WalletBalance = customer.Wallet?.Balance ?? 0m,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt
            };
        }
    }
}
