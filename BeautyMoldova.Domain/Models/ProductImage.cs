namespace BeautyMoldova.Domain.Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ImageUrl { get; set; }
        public string AltText { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        
        public virtual Product Product { get; set; }
    }
} 