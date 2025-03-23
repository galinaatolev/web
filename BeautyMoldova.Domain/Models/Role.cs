using System;
using System.Collections.Generic;

namespace BeautyMoldova.Domain.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Permissions { get; set; } // JSON строка с разрешениями

        public virtual ICollection<CustomerRole> CustomerRoles { get; set; }
    }
} 