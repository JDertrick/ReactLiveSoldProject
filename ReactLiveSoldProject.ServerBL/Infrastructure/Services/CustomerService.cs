using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Helpers;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.CustomerWallet;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly LiveSoldDbContext _dbContext;
        private readonly IMapper _mapper;

        public CustomerService(LiveSoldDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<CustomerDto>> GetCustomersByOrganizationAsync(Guid organizationId)
        {
            var customers = await _dbContext.Customers
                .Include(c => c.Wallet)
                .Include(c => c.AssignedSeller)
                .Where(c => c.OrganizationId == organizationId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var c = _mapper.Map<List<CustomerDto>>(customers);

            return c;
        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(Guid customerId, Guid organizationId)
        {
            var customer = await _dbContext.Customers
                .Include(c => c.Wallet)
                .Include(c => c.AssignedSeller)
                .FirstOrDefaultAsync(c => c.Id == customerId && c.OrganizationId == organizationId);

            return customer != null ? _mapper.Map<CustomerDto>(customer) : null;
        }

        public async Task<List<CustomerDto>> SearchCustomersAsync(Guid organizationId, string searchTerm)
        {
            var normalizedSearch = searchTerm.ToLower().Trim();

            var customers = await _dbContext.Customers
                .Include(c => c.Wallet)
                .Include(c => c.AssignedSeller)
                .Where(c => c.OrganizationId == organizationId &&
                    (c.Email.ToLower().Contains(normalizedSearch) ||
                     c.FirstName != null && c.FirstName.ToLower().Contains(normalizedSearch) ||
                     c.LastName != null && c.LastName.ToLower().Contains(normalizedSearch) ||
                     c.Phone != null && c.Phone.Contains(normalizedSearch)))
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

            var customer = _mapper.Map<Customer>(dto);
            customer.Id = customerId;
            customer.PasswordHash = PasswordHelper.HashPassword(dto.Password);
            customer.CreatedAt = DateTime.UtcNow;
            customer.UpdatedAt = DateTime.UtcNow;

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
            customer = _mapper.Map<Customer>(dto);
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

        public async Task<CustomerDto> RegisterCustomerAsync(RegisterCustomerDto dto)
        {
            // Obtener la organización por slug
            var organization = await _dbContext.Organizations
                .FirstOrDefaultAsync(o => o.Slug == dto.OrganizationSlug);

            if (organization == null)
                throw new KeyNotFoundException("Organización no encontrada");

            // Verificar que no exista un cliente con el mismo email en esta organización
            var existingCustomer = await _dbContext.Customers
                .FirstOrDefaultAsync(c => c.OrganizationId == organization.Id && c.Email == dto.Email);

            if (existingCustomer != null)
                throw new InvalidOperationException("Ya existe una cuenta con este email");

            // Si tiene teléfono, verificar que tampoco exista
            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                var existingPhone = await _dbContext.Customers
                    .FirstOrDefaultAsync(c => c.OrganizationId == organization.Id && c.Phone == dto.Phone);

                if (existingPhone != null)
                    throw new InvalidOperationException("Ya existe una cuenta con este teléfono");
            }

            // Crear el cliente
            var customerId = Guid.NewGuid();
            var customer = new Customer
            {
                Id = customerId,
                OrganizationId = organization.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                PasswordHash = PasswordHelper.HashPassword(dto.Password),
                AssignedSellerId = null, // No hay vendedor asignado en registro público
                Notes = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Customers.Add(customer);

            // Crear automáticamente el wallet para el cliente
            var wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                CustomerId = customerId,
                Balance = 0.00m,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Wallets.Add(wallet);

            await _dbContext.SaveChangesAsync();

            // Recargar con wallet incluido
            var createdCustomer = await _dbContext.Customers
                .Include(c => c.Wallet)
                .FirstAsync(c => c.Id == customerId);

            return MapToDto(createdCustomer);
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
