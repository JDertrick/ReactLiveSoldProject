using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Contacts;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    public class ContactService : IContactService
    {
        private readonly LiveSoldDbContext _dbContext;
        private readonly IMapper _mapper;

        public ContactService(LiveSoldDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<ContactDto>> GetContactsByOrganizationAsync(Guid organizationId)
        {
            var contacts = await _dbContext.Contacts
                .Where(c => c.OrganizationId == organizationId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<ContactDto>>(contacts);
        }

        public async Task<ContactDto?> GetContactByIdAsync(Guid contactId, Guid organizationId)
        {
            var contact = await _dbContext.Contacts
                .FirstOrDefaultAsync(c => c.Id == contactId && c.OrganizationId == organizationId);

            return contact != null ? _mapper.Map<ContactDto>(contact) : null;
        }

        public async Task<List<ContactDto>> SearchContactsAsync(Guid organizationId, string searchTerm)
        {
            var normalizedSearch = searchTerm.ToLower().Trim();

            var contacts = await _dbContext.Contacts
                .Where(c => c.OrganizationId == organizationId &&
                    (c.Email.ToLower().Contains(normalizedSearch) ||
                     c.FirstName != null && c.FirstName.ToLower().Contains(normalizedSearch) ||
                     c.LastName != null && c.LastName.ToLower().Contains(normalizedSearch) ||
                     c.Phone != null && c.Phone.Contains(normalizedSearch) ||
                     c.Company != null && c.Company.ToLower().Contains(normalizedSearch)))
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<ContactDto>>(contacts);
        }

        public async Task<ContactDto> CreateContactAsync(Guid organizationId, CreateContactDto dto)
        {
            // Verificar que no exista un contacto con el mismo email en esta organización
            var existingContact = await _dbContext.Contacts
                .FirstOrDefaultAsync(c => c.OrganizationId == organizationId && c.Email == dto.Email);

            if (existingContact != null)
                throw new InvalidOperationException("Ya existe un contacto con este email en la organización");

            // Si tiene teléfono, verificar que tampoco exista
            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                var existingPhone = await _dbContext.Contacts
                    .FirstOrDefaultAsync(c => c.OrganizationId == organizationId && c.Phone == dto.Phone);

                if (existingPhone != null)
                    throw new InvalidOperationException("Ya existe un contacto con este teléfono en la organización");
            }

            // Crear el contacto
            var contact = _mapper.Map<Contact>(dto);
            contact.Id = Guid.NewGuid();
            contact.OrganizationId = organizationId;

            _dbContext.Contacts.Add(contact);
            await _dbContext.SaveChangesAsync();

            return _mapper.Map<ContactDto>(contact);
        }

        public async Task<ContactDto> UpdateContactAsync(Guid contactId, Guid organizationId, UpdateContactDto dto)
        {
            var contact = await _dbContext.Contacts
                .FirstOrDefaultAsync(c => c.Id == contactId && c.OrganizationId == organizationId);

            if (contact == null)
                throw new KeyNotFoundException("Contacto no encontrado");

            // Verificar email único (excluyendo este contacto)
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != contact.Email)
            {
                var emailExists = await _dbContext.Contacts
                    .AnyAsync(c => c.OrganizationId == organizationId && c.Email == dto.Email && c.Id != contactId);

                if (emailExists)
                    throw new InvalidOperationException("Ya existe otro contacto con este email en la organización");
            }

            // Verificar teléfono único (excluyendo este contacto)
            if (!string.IsNullOrWhiteSpace(dto.Phone) && dto.Phone != contact.Phone)
            {
                var phoneExists = await _dbContext.Contacts
                    .AnyAsync(c => c.OrganizationId == organizationId && c.Phone == dto.Phone && c.Id != contactId);

                if (phoneExists)
                    throw new InvalidOperationException("Ya existe otro contacto con este teléfono en la organización");
            }

            // Actualizar campos
            if (!string.IsNullOrWhiteSpace(dto.Email))
                contact.Email = dto.Email;

            if (dto.FirstName != null)
                contact.FirstName = dto.FirstName;

            if (dto.LastName != null)
                contact.LastName = dto.LastName;

            if (dto.Phone != null)
                contact.Phone = dto.Phone;

            if (dto.Address != null)
                contact.Address = dto.Address;

            if (dto.City != null)
                contact.City = dto.City;

            if (dto.State != null)
                contact.State = dto.State;

            if (dto.PostalCode != null)
                contact.PostalCode = dto.PostalCode;

            if (dto.Country != null)
                contact.Country = dto.Country;

            if (dto.Company != null)
                contact.Company = dto.Company;

            if (dto.JobTitle != null)
                contact.JobTitle = dto.JobTitle;

            if (dto.IsActive.HasValue)
                contact.IsActive = dto.IsActive.Value;

            contact.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return _mapper.Map<ContactDto>(contact);
        }

        public async Task DeleteContactAsync(Guid contactId, Guid organizationId)
        {
            var contact = await _dbContext.Contacts
                .FirstOrDefaultAsync(c => c.Id == contactId && c.OrganizationId == organizationId);

            if (contact == null)
                throw new KeyNotFoundException("Contacto no encontrado");

            // Verificar si está asociado a clientes
            var hasCustomers = await _dbContext.Customers
                .AnyAsync(c => c.ContactId == contactId);

            if (hasCustomers)
            {
                throw new InvalidOperationException(
                    "No se puede eliminar el contacto porque está asociado a clientes. " +
                    "Considere desactivarlo en lugar de eliminarlo.");
            }

            // Verificar si está asociado a proveedores
            var hasVendors = await _dbContext.Vendors
                .AnyAsync(v => v.ContactId == contactId);

            if (hasVendors)
            {
                throw new InvalidOperationException(
                    "No se puede eliminar el contacto porque está asociado a proveedores. " +
                    "Considere desactivarlo en lugar de eliminarlo.");
            }

            _dbContext.Contacts.Remove(contact);
            await _dbContext.SaveChangesAsync();
        }
    }
}
