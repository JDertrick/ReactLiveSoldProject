using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Models.Authentication;

namespace ReactLiveSoldProject.ServerBL.Models.Contacts
{
    public class Contact
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El ID de la organización es obligatorio")]
        public Guid OrganizationId { get; set; }

        public virtual Organization Organization { get; set; }

        [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string? FirstName { get; set; }

        [MaxLength(100, ErrorMessage = "El apellido no puede exceder los 100 caracteres")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [MaxLength(255, ErrorMessage = "El email no puede exceder los 255 caracteres")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        [MaxLength(20, ErrorMessage = "El teléfono no puede exceder los 20 caracteres")]
        public string? Phone { get; set; }

        [MaxLength(500, ErrorMessage = "La dirección no puede exceder los 500 caracteres")]
        public string? Address { get; set; }

        [MaxLength(100, ErrorMessage = "La ciudad no puede exceder los 100 caracteres")]
        public string? City { get; set; }

        [MaxLength(100, ErrorMessage = "El estado/provincia no puede exceder los 100 caracteres")]
        public string? State { get; set; }

        [MaxLength(20, ErrorMessage = "El código postal no puede exceder los 20 caracteres")]
        public string? PostalCode { get; set; }

        [MaxLength(100, ErrorMessage = "El país no puede exceder los 100 caracteres")]
        public string? Country { get; set; }

        [MaxLength(100, ErrorMessage = "La empresa no puede exceder los 100 caracteres")]
        public string? Company { get; set; }

        [MaxLength(100, ErrorMessage = "El puesto no puede exceder los 100 caracteres")]
        public string? JobTitle { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
