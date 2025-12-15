using Mapster;
using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Helpers;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Contacts;
using ReactLiveSoldProject.ServerBL.Models.CustomerWallet;
using ReactLiveSoldProject.ServerBL.Models.Configuration;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly LiveSoldDbContext _dbContext;
        private readonly ISerieNoService _serieNoService;

        public CustomerService(LiveSoldDbContext dbContext, ISerieNoService serieNoService)
        {
            _dbContext = dbContext;
            _serieNoService = serieNoService;
        }

        public async Task<List<CustomerDto>> GetCustomersByOrganizationAsync(Guid organizationId)
        {
            var customers = await _dbContext.Customers
                .Include(c => c.Contact)
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
                .Include(c => c.Contact)
                .Include(c => c.Wallet)
                .Include(c => c.AssignedSeller)
                .FirstOrDefaultAsync(c => c.Id == customerId && c.OrganizationId == organizationId);

            return customer != null ? MapToDto(customer) : null;
        }

        public async Task<List<CustomerDto>> SearchCustomersAsync(Guid organizationId, string searchTerm)
        {
            var normalizedSearch = searchTerm.ToLower().Trim();

            var customers = await _dbContext.Customers
                .Include(c => c.Contact)
                .Include(c => c.Wallet)
                .Include(c => c.AssignedSeller)
                .Where(c => c.OrganizationId == organizationId &&
                    (c.Contact.Email.ToLower().Contains(normalizedSearch) ||
                     c.Contact.FirstName != null && c.Contact.FirstName.ToLower().Contains(normalizedSearch) ||
                     c.Contact.LastName != null && c.Contact.LastName.ToLower().Contains(normalizedSearch) ||
                     c.Contact.Phone != null && c.Contact.Phone.Contains(normalizedSearch)))
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return customers.Select(c => MapToDto(c)).ToList();
        }

        public async Task<CustomerDto> CreateCustomerAsync(Guid organizationId, CreateCustomerDto dto)
        {
            try
            {
                var contact = _dbContext.Contacts
                    .Where(c => c.OrganizationId == organizationId && c.Email == dto.Email)
                    .FirstOrDefault();

                if (contact == null)
                    throw new InvalidOperationException("El contacto no existe");

                var customer = _dbContext.Customers
                    .Where(c => c.OrganizationId == organizationId && c.ContactId == contact.Id)
                    .FirstOrDefault();

                if (customer != null)
                    throw new InvalidOperationException($"Esta informacion de contacto ya se encuentra registrada en la organizacion");

                // Generar número de cliente usando series numéricas
                var customerNo = await _serieNoService.GetNextNumberByTypeAsync(organizationId, DocumentType.Customer);

                // Crear el cliente
                var customerId = Guid.NewGuid();
                var newCustomer = new Customer
                {
                    Id = customerId,
                    OrganizationId = organizationId,
                    CustomerNo = customerNo,
                    ContactId = contact.Id,
                    PasswordHash = PasswordHelper.HashPassword(dto.Password),
                    AssignedSellerId = dto.AssignedSellerId,
                    Notes = dto.Notes,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.Customers.Add(newCustomer);

                // Crear automáticamente el wallet para el cliente
                var wallet = new Wallet
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    CustomerId = customerId,
                    Balance = 0.00m,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.Wallets.Add(wallet);

                await _dbContext.SaveChangesAsync();

                // Recargar el customer con sus relaciones
                var createdCustomer = await _dbContext.Customers
                    .Include(c => c.Contact)
                    .Include(c => c.Wallet)
                    .Include(c => c.AssignedSeller)
                    .FirstAsync(c => c.Id == customerId);

                return createdCustomer.Adapt<CustomerDto>();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<CustomerDto> UpdateCustomerAsync(Guid customerId, Guid organizationId, UpdateCustomerDto dto)
        {
            var customer = await _dbContext.Customers
                .Include(c => c.Contact)
                .Include(c => c.Wallet)
                .Include(c => c.AssignedSeller)
                .FirstOrDefaultAsync(c => c.Id == customerId && c.OrganizationId == organizationId);

            if (customer == null)
                throw new KeyNotFoundException("Cliente no encontrado");

            // Verificar email único (excluyendo este contacto)
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != customer.Contact.Email)
            {
                var emailExists = await _dbContext.Contacts
                    .AnyAsync(c => c.OrganizationId == organizationId && c.Email == dto.Email && c.Id != customer.ContactId);

                if (emailExists)
                    throw new InvalidOperationException("Ya existe otro contacto con este email en la organización");
            }

            // Verificar teléfono único (excluyendo este contacto)
            if (!string.IsNullOrWhiteSpace(dto.Phone) && dto.Phone != customer.Contact.Phone)
            {
                var phoneExists = await _dbContext.Contacts
                    .AnyAsync(c => c.OrganizationId == organizationId && c.Phone == dto.Phone && c.Id != customer.ContactId);

                if (phoneExists)
                    throw new InvalidOperationException("Ya existe otro contacto con este teléfono en la organización");
            }

            // Verificar que el assigned seller pertenezca a la organización (si se proporciona)
            if (dto.AssignedSellerId.HasValue)
            {
                var sellerExists = await _dbContext.OrganizationMembers
                    .AnyAsync(om => om.OrganizationId == organizationId && om.UserId == dto.AssignedSellerId.Value);

                if (!sellerExists)
                    throw new InvalidOperationException("El vendedor asignado no pertenece a esta organización");
            }

            // Actualizar campos del Contact
            if (!string.IsNullOrWhiteSpace(dto.Email))
                customer.Contact.Email = dto.Email;

            if (dto.FirstName != null)
                customer.Contact.FirstName = dto.FirstName;

            if (dto.LastName != null)
                customer.Contact.LastName = dto.LastName;

            if (dto.Phone != null)
                customer.Contact.Phone = dto.Phone;

            if (dto.Address != null)
                customer.Contact.Address = dto.Address;

            if (dto.City != null)
                customer.Contact.City = dto.City;

            if (dto.State != null)
                customer.Contact.State = dto.State;

            if (dto.PostalCode != null)
                customer.Contact.PostalCode = dto.PostalCode;

            if (dto.Country != null)
                customer.Contact.Country = dto.Country;

            if (dto.Company != null)
                customer.Contact.Company = dto.Company;

            customer.Contact.UpdatedAt = DateTime.UtcNow;

            // Actualizar campos del Customer
            if (dto.AssignedSellerId.HasValue)
                customer.AssignedSellerId = dto.AssignedSellerId;

            if (dto.Notes != null)
                customer.Notes = dto.Notes;

            if (dto.IsActive.HasValue)
            {
                customer.IsActive = dto.IsActive.Value;
                customer.Contact.IsActive = dto.IsActive.Value;
            }

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
                .Include(c => c.Contact)
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

            // Eliminar el customer (el wallet se eliminará en cascada)
            _dbContext.Customers.Remove(customer);

            // Eliminar el contacto asociado
            _dbContext.Contacts.Remove(customer.Contact);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<CustomerDto> RegisterCustomerAsync(RegisterCustomerDto dto)
        {
            // Obtener la organización por slug
            var organization = await _dbContext.Organizations
                .FirstOrDefaultAsync(o => o.Slug == dto.OrganizationSlug);

            if (organization == null)
                throw new KeyNotFoundException("Organización no encontrada");

            // Verificar que no exista un contacto con el mismo email en esta organización
            var existingContact = await _dbContext.Contacts
                .FirstOrDefaultAsync(c => c.OrganizationId == organization.Id && c.Email == dto.Email);

            if (existingContact != null)
                throw new InvalidOperationException("Ya existe una cuenta con este email");

            // Si tiene teléfono, verificar que tampoco exista
            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                var existingPhone = await _dbContext.Contacts
                    .FirstOrDefaultAsync(c => c.OrganizationId == organization.Id && c.Phone == dto.Phone);

                if (existingPhone != null)
                    throw new InvalidOperationException("Ya existe una cuenta con este teléfono");
            }

            // Crear el contacto primero
            var contact = new Contact
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Contacts.Add(contact);

            // Crear el cliente
            var customerId = Guid.NewGuid();
            var customer = new Customer
            {
                Id = customerId,
                OrganizationId = organization.Id,
                ContactId = contact.Id,
                PasswordHash = PasswordHelper.HashPassword(dto.Password),
                AssignedSellerId = null, // No hay vendedor asignado en registro público
                Notes = null,
                IsActive = true,
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
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Wallets.Add(wallet);

            await _dbContext.SaveChangesAsync();

            // Recargar con wallet y contact incluidos
            var createdCustomer = await _dbContext.Customers
                .Include(c => c.Contact)
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
                ContactId = customer.ContactId,
                FirstName = customer.Contact?.FirstName,
                LastName = customer.Contact?.LastName,
                Email = customer.Contact?.Email ?? string.Empty,
                Phone = customer.Contact?.Phone,
                AssignedSellerId = customer.AssignedSellerId,
                AssignedSellerName = customer.AssignedSeller != null
                    ? $"{customer.AssignedSeller.FirstName} {customer.AssignedSeller.LastName}".Trim()
                    : null,
                Notes = customer.Notes,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt,
                IsActive = customer.IsActive,
                Wallet = customer.Wallet != null ? new WalletDto
                {
                    Id = customer.Wallet.Id,
                    CustomerId = customer.Wallet.CustomerId,
                    CustomerName = $"{customer.Contact?.FirstName} {customer.Contact?.LastName}".Trim(),
                    CustomerEmail = customer.Contact?.Email ?? string.Empty,
                    Balance = customer.Wallet.Balance,
                    UpdatedAt = customer.Wallet.UpdatedAt
                } : null,
                Contact = customer.Contact != null ? new ContactDto
                {
                    Id = customer.Contact.Id,
                    OrganizationId = customer.Contact.OrganizationId,
                    FirstName = customer.Contact.FirstName,
                    LastName = customer.Contact.LastName,
                    Email = customer.Contact.Email,
                    Phone = customer.Contact.Phone,
                    Address = customer.Contact.Address,
                    City = customer.Contact.City,
                    State = customer.Contact.State,
                    PostalCode = customer.Contact.PostalCode,
                    Country = customer.Contact.Country,
                    Company = customer.Contact.Company,
                    JobTitle = customer.Contact.JobTitle,
                    CreatedAt = customer.Contact.CreatedAt,
                    UpdatedAt = customer.Contact.UpdatedAt,
                    IsActive = customer.Contact.IsActive
                } : null
            };
        }
    }
}
