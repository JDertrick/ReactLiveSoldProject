using System.ComponentModel.DataAnnotations;

namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class CreateCustomerDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [Phone]
        [MaxLength(20)]
        public string? Phone { get; set; }

        public Guid? AssignedSellerId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
