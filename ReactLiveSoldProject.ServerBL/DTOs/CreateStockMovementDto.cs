using System.ComponentModel.DataAnnotations;

namespace ReactLiveSoldProject.ServerBL.DTOs
{
    public class CreateStockMovementDto
    {
        [Required(ErrorMessage = "El ID de la variante del producto es obligatorio")]
        public Guid ProductVariantId { get; set; }

        [Required(ErrorMessage = "El tipo de movimiento es obligatorio")]
        public string MovementType { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        public int Quantity { get; set; }

        [MaxLength(500, ErrorMessage = "Las notas no pueden exceder los 500 caracteres")]
        public string? Notes { get; set; }

        [MaxLength(100, ErrorMessage = "La referencia no puede exceder los 100 caracteres")]
        public string? Reference { get; set; }

        public decimal? UnitCost { get; set; }
    }
}
