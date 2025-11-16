using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Models.Authentication;

namespace ReactLiveSoldProject.ServerBL.Models.Inventory
{
    public class Location
    {
        public Guid Id { get; set; }

        [Required]
        public Guid OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
