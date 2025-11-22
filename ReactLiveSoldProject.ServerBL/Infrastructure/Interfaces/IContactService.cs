using ReactLiveSoldProject.ServerBL.DTOs;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface IContactService
    {
        /// <summary>
        /// Obtiene todos los contactos de una organización
        /// </summary>
        Task<List<ContactDto>> GetContactsByOrganizationAsync(Guid organizationId);

        /// <summary>
        /// Obtiene un contacto por ID (validando que pertenezca a la organización)
        /// </summary>
        Task<ContactDto?> GetContactByIdAsync(Guid contactId, Guid organizationId);

        /// <summary>
        /// Busca contactos por email, nombre o empresa dentro de una organización
        /// </summary>
        Task<List<ContactDto>> SearchContactsAsync(Guid organizationId, string searchTerm);

        /// <summary>
        /// Crea un nuevo contacto
        /// </summary>
        Task<ContactDto> CreateContactAsync(Guid organizationId, CreateContactDto dto);

        /// <summary>
        /// Actualiza un contacto existente
        /// </summary>
        Task<ContactDto> UpdateContactAsync(Guid contactId, Guid organizationId, UpdateContactDto dto);

        /// <summary>
        /// Elimina un contacto (solo si no está asociado a clientes o proveedores)
        /// </summary>
        Task DeleteContactAsync(Guid contactId, Guid organizationId);
    }
}
