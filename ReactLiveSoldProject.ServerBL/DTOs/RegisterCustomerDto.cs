using System.ComponentModel.DataAnnotations;

namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class RegisterCustomerDto
    {
        [Required(ErrorMessage = "El slug de la organización es requerido")]
        public string OrganizationSlug { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string? FirstName { get; set; }

        [MaxLength(100, ErrorMessage = "El apellido no puede exceder los 100 caracteres")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [MaxLength(255, ErrorMessage = "El email no puede exceder los 255 caracteres")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        [MaxLength(20, ErrorMessage = "El teléfono no puede exceder los 20 caracteres")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
