using System;
using System.Collections.Generic;

namespace BeautyMoldova.Domain.Models
{
    public class Manufacturer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Logo { get; set; }
        public string Website { get; set; }
        public string Country { get; set; }
        public bool IsLuxury { get; set; }
        public DateTime EstablishedYear { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<Product> Products { get; set; }
        public virtual ICollection<Banner> Banners { get; set; }
    }
} 