using System;

namespace BeautyMoldova.Domain.Models
{
    public class Banner
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string LinkUrl { get; set; }
        public int DisplayOrder { get; set; }
        public string Position { get; set; } // Header, Sidebar, Footer, etc.
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? TargetCategoryId { get; set; }
        public int? TargetManufacturerId { get; set; }
        public int ViewCount { get; set; }
        public int ClickCount { get; set; }

        public virtual Category TargetCategory { get; set; }
        public virtual Manufacturer TargetManufacturer { get; set; }
    }
} 