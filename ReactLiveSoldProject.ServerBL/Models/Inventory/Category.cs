using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReactLiveSoldProject.ServerBL.Models.Authentication;

namespace ReactLiveSoldProject.ServerBL.Models.Inventory
{
    public class Category
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

        public Guid? ParentId { get; set; }
        [ForeignKey("ParentId")]
        public virtual Category Parent { get; set; }

        public virtual ICollection<Category> Children { get; set; } = new List<Category>();

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
