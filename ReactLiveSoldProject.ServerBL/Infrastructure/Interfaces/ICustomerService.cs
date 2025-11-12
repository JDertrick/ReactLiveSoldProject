using ReactLiveSoldProject.ServerBL.DTOs;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface ICustomerService
    {
        /// <summary>
        /// Obtiene todos los clientes de una organización
        /// </summary>
        Task<List<CustomerDto>> GetCustomersByOrganizationAsync(Guid organizationId);

        /// <summary>
        /// Obtiene un cliente por ID (validando que pertenezca a la organización)
        /// </summary>
        Task<CustomerDto?> GetCustomerByIdAsync(Guid customerId, Guid organizationId);

        /// <summary>
        /// Busca clientes por email o nombre dentro de una organización
        /// </summary>
        Task<List<CustomerDto>> SearchCustomersAsync(Guid organizationId, string searchTerm);

        /// <summary>
        /// Crea un nuevo cliente y automáticamente crea su wallet
        /// </summary>
        Task<CustomerDto> CreateCustomerAsync(Guid organizationId, CreateCustomerDto dto);

        /// <summary>
        /// Actualiza un cliente existente
        /// </summary>
        Task<CustomerDto> UpdateCustomerAsync(Guid customerId, Guid organizationId, UpdateCustomerDto dto);

        /// <summary>
        /// Elimina un cliente (solo si no tiene órdenes o transacciones)
        /// </summary>
        Task DeleteCustomerAsync(Guid customerId, Guid organizationId);
    }
}
