using System.ComponentModel.DataAnnotations;

namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El password es obligatorio")]
        [MinLength(6, ErrorMessage = "El password debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;
    }
}
