using System.ComponentModel.DataAnnotations;

namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class CreateSalesOrderDto
    {
        [Required]
        public Guid CustomerId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "La orden debe tener al menos un item")]
        public List<CreateSalesOrderItemDto> Items { get; set; } = new();
    }
}
