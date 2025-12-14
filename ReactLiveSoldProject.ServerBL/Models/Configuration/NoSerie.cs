using System.ComponentModel.DataAnnotations;

namespace ReactLiveSoldProject.ServerBL.Models.Configuration
{
    public class NoSerie
    {
        public Guid Id { get; set; }

        public Guid OrganizationId { get; set; }

        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de documento al que aplica esta serie (Customer, Vendor, Invoice, etc.)
        /// </summary>
        public DocumentType? DocumentType { get; set; }

        /// <summary>
        /// Si es true, esta será la serie por defecto para el tipo de documento
        /// </summary>
        public bool DefaultNos { get; set; }

        /// <summary>
        /// Si es true, permite ingresar números manualmente en lugar de usar la secuencia
        /// </summary>
        public bool ManualNos { get; set; }

        /// <summary>
        /// Si es true, valida que las fechas de los documentos estén en orden
        /// </summary>
        public bool DateOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<NoSerieLine> NoSerieLines { get; set; } = new List<NoSerieLine>();
    }
}