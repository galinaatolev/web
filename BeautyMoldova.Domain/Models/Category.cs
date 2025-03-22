using System.Collections.Generic;

namespace BeautyMoldova.Domain.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int? ParentCategoryId { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public string Slug { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }

        public virtual Category ParentCategory { get; set; }
        public virtual ICollection<Category> ChildCategories { get; set; }
        public virtual ICollection<Product> Products { get; set; }
        public virtual ICollection<Banner> Banners { get; set; }
    }
} 