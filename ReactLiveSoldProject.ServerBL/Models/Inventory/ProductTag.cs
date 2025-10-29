namespace ReactLiveSoldProject.ServerBL.Models.Inventory
{
    public class ProductTag
    {
        // Esta tabla usa una clave primaria compuesta (product_id, tag_id).
        // Se configura en el DbContext.
        public Guid ProductId { get; set; }
        
        public virtual Product Product { get; set; }
        
        public Guid TagId { get; set; }
        
        public virtual Tag Tag { get; set; }
    }
}
