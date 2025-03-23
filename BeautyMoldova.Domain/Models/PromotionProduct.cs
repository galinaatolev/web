namespace BeautyMoldova.Domain.Models
{
    public class PromotionProduct
    {
        public int Id { get; set; }
        public int PromotionId { get; set; }
        public int ProductId { get; set; }
        
        public virtual Promotion Promotion { get; set; }
        public virtual Product Product { get; set; }
    }
} 