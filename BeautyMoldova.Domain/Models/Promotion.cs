using System;
using System.Collections.Generic;

namespace BeautyMoldova.Domain.Models
{
    public class Promotion
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string PromoCode { get; set; }
        public string Description { get; set; }
        public string DiscountType { get; set; } // Percentage, FixedAmount
        public decimal DiscountValue { get; set; }
        public decimal? MinimumOrderAmount { get; set; }
        public int? MaximumUses { get; set; }
        public int UsageCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool ApplyToAllProducts { get; set; }
        public string ImageUrl { get; set; }
        public int DiscountPercent { get; set; }
        
        public virtual ICollection<PromotionProduct> PromotionProducts { get; set; }
    }
} 