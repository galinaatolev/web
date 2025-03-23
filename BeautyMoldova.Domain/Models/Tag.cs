using System.Collections.Generic;

namespace BeautyMoldova.Domain.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public bool IsActive { get; set; }
        
        public virtual ICollection<ProductTag> ProductTags { get; set; }
    }
} 