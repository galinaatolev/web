using System;
using System.Collections.Generic;

namespace BeautyMoldova.Domain.Models
{
    public class Purchase
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime? ShippingDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string Status { get; set; } // Pending, Processing, Shipped, Delivered, Cancelled
        public decimal TotalAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public string PromoCode { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; } // Pending, Completed, Failed, Refunded
        public string ShippingMethod { get; set; }
        public string TrackingNumber { get; set; }
        public string ShippingAddress { get; set; }
        public string BillingAddress { get; set; }
        public string CustomerNotes { get; set; }
        public string AdminNotes { get; set; }
        public string Notes { get; set; }
        public int? AssignedEmployeeId { get; set; }
        
        public virtual Customer Customer { get; set; }
        public virtual ICollection<PurchaseItem> PurchaseItems { get; set; }
    }
} 