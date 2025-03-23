using System;

namespace BeautyMoldova.Domain.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public int Rating { get; set; } // 1-5
        public string Title { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsVerifiedPurchase { get; set; }
        public bool IsApproved { get; set; }
        public bool IsDeleted { get; set; }
        public string AdminResponse { get; set; }
        public DateTime? AdminResponseDate { get; set; }
        public int? HelpfulVotes { get; set; }
        public int? UnhelpfulVotes { get; set; }
        
        public virtual Product Product { get; set; }
        public virtual Customer Customer { get; set; }
    }
} 