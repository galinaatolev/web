using System;

namespace BeautyMoldova.Domain.Models
{
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }
} 