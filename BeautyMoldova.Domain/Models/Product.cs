using System;
using System.Collections.Generic;

namespace BeautyMoldova.Domain.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string SKU { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int StockQuantity { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsFeatured { get; set; }
        public int ManufacturerId { get; set; }
        public int CategoryId { get; set; }
        public string MainImage { get; set; }
        public DateTime AddedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string Ingredients { get; set; }
        public string HowToUse { get; set; }
        public int? Size { get; set; }
        public string SizeUnit { get; set; }
        public decimal? Weight { get; set; }
        public string SkinType { get; set; }
        public string ApplicationArea { get; set; }
        public string VolumeWeight { get; set; }
        public int ViewCount { get; set; }
        
        public virtual Manufacturer Manufacturer { get; set; }
        public virtual Category Category { get; set; }
        public virtual ICollection<ProductImage> AdditionalImages { get; set; }
        public virtual ICollection<PurchaseItem> PurchaseItems { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<WishlistItem> WishlistItems { get; set; }
        public virtual ICollection<ProductTag> ProductTags { get; set; }
    }
} 