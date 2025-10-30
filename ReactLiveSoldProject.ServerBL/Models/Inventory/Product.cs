﻿using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.Models.Authentication;

namespace ReactLiveSoldProject.ServerBL.Models.Inventory
{
    public class Product
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El ID de la organización es obligatorio")]
        public Guid OrganizationId { get; set; }

        public virtual Organization Organization { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        [MaxLength(255, ErrorMessage = "El nombre no puede exceder los 255 caracteres")]
        public string Name { get; set; }

        [MaxLength(2000, ErrorMessage = "La descripción no puede exceder los 2000 caracteres")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "El tipo de producto es obligatorio")]
        public ProductType ProductType { get; set; } = ProductType.Simple;

        public bool IsPublished { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación
        public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        
        public virtual ICollection<ProductTag> TagLinks { get; set; } = new List<ProductTag>();
    }
}
