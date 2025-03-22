using System;

namespace BeautyMoldova.Domain.Models
{
    public class CustomerRole
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int RoleId { get; set; }
        public DateTime AssignedDate { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Role Role { get; set; }
    }
} 