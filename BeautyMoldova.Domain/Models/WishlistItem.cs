using System;

namespace BeautyMoldova.Domain.Models
{
    public class WishlistItem
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public DateTime AddedAt { get; set; }
        public DateTime AddedDate { get; set; }
        public string Notes { get; set; }
        public int Priority { get; set; } // 1-5
        
        public virtual Customer Customer { get; set; }
        public virtual Product Product { get; set; }
    }
} 