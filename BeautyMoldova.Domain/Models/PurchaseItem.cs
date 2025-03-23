using System;

namespace BeautyMoldova.Domain.Models
{
    public class PurchaseItem
    {
        public int Id { get; set; }
        public int PurchaseId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSKU { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public bool IsGift { get; set; }
        public string GiftMessage { get; set; }
        public bool ReturnRequested { get; set; }
        public DateTime? ReturnRequestDate { get; set; }
        public string ReturnReason { get; set; }
        public string ReturnStatus { get; set; }
        
        public virtual Purchase Purchase { get; set; }
        public virtual Product Product { get; set; }
    }
} 