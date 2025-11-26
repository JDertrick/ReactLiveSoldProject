using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.Models.Authentication;

namespace ReactLiveSoldProject.ServerBL.Models.Audit
{
    /// <summary>
    /// Flujo de aprobación para documentos que requieren autorización
    /// (Órdenes de compra, Pagos, etc.)
    /// </summary>
    public class ApprovalWorkflow
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El ID de la organización es obligatorio")]
        public Guid OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }

        [Required(ErrorMessage = "El tipo de documento es obligatorio")]
        [MaxLength(50, ErrorMessage = "El tipo de documento no puede exceder los 50 caracteres")]
        public string DocumentType { get; set; } // "PurchaseOrder", "Payment", "VendorInvoice"

        [Required(ErrorMessage = "El ID del documento es obligatorio")]
        public Guid DocumentId { get; set; } // ID del documento que requiere aprobación

        [Required(ErrorMessage = "El ID del solicitante es obligatorio")]
        public Guid RequesterId { get; set; }
        public virtual User Requester { get; set; }

        public Guid? ApproverId { get; set; }
        public virtual User? Approver { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        [Required(ErrorMessage = "La fecha de solicitud es obligatoria")]
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        public DateTime? ResponseDate { get; set; } // Fecha de aprobación/rechazo

        [MaxLength(1000, ErrorMessage = "Los comentarios no pueden exceder los 1000 caracteres")]
        public string? Comments { get; set; } // Comentarios del aprobador

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
