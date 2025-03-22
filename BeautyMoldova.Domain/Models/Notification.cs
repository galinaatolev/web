using System;

namespace BeautyMoldova.Domain.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } // Info, Warning, Success, Error
        public bool IsRead { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ReadDate { get; set; }
        public string RelatedEntityType { get; set; } // Product, Order, etc.
        public int? RelatedEntityId { get; set; }

        public virtual Customer Customer { get; set; }
    }
} 