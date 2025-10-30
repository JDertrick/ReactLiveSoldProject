using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.Models.Authentication;

namespace ReactLiveSoldProject.ServerBL.Models.Audit
{
    public class AuditLog
    {
        public Guid Id { get; set; }

        public Guid? OrganizationId { get; set; } // Puede ser nulo para acciones de super-admin

        public virtual Organization? Organization { get; set; }

        public Guid? UserId { get; set; } // Puede ser nulo para acciones del sistema

        public virtual User? User { get; set; }

        public AuditActionType ActionType { get; set; }

        public string TargetTable { get; set; }

        public Guid? TargetRecordId { get; set; }

        public string? Changes { get; set; } // Almacena el 'before' y 'after' como JSON

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
