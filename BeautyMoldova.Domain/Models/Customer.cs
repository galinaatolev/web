using System;
using System.Collections.Generic;

namespace BeautyMoldova.Domain.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Username { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string AccessLevel { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool IsActive { get; set; }
        public string ShippingAddress { get; set; }
        public string BillingAddress { get; set; }
        public int LoyaltyPoints { get; set; }
        public virtual ICollection<Purchase> Purchases { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<WishlistItem> WishlistItems { get; set; }
        public virtual ICollection<CustomerRole> CustomerRoles { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
    }
} 