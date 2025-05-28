using System.Collections.Generic;
using BeautyMoldova.Domain.Models;

namespace BeautyMoldova.Application.Interfaces
{
    public interface ICustomerBL
    {
        // Основные операции с клиентами
        Customer GetCustomerById(int id);
        Customer GetCustomerByUsername(string username);
        Customer GetCustomerByEmail(string email);
        List<Customer> GetAllCustomers();
        List<Customer> SearchCustomers(string searchTerm);
        
        // CRUD операции
        bool CreateCustomer(Customer customer);
        bool UpdateCustomer(Customer customer);
        bool DeleteCustomer(int id);
        
        // Дополнительные операции
        int GetCustomersCount();
        List<Customer> GetPagedCustomers(int page, int pageSize);
        bool IsEmailExists(string email);
        bool IsUsernameExists(string username);
        
        // Операции с ролями
        bool AssignRole(int customerId, int roleId);
        bool RemoveRole(int customerId, int roleId);
        List<Role> GetCustomerRoles(int customerId);
        bool HasRole(int customerId, string roleName);
        
        // Операции с заказами
        List<Purchase> GetCustomerPurchases(int customerId);
        int GetCustomerPurchasesCount(int customerId);
        
        // Операции с отзывами
        List<Review> GetCustomerReviews(int customerId);
        int GetCustomerReviewsCount(int customerId);
        
        // Операции с wishlist
        List<WishlistItem> GetCustomerWishlist(int customerId);
        bool AddToWishlist(int customerId, int productId);
        bool RemoveFromWishlist(int customerId, int productId);
        
        // Операции с уведомлениями
        List<Notification> GetCustomerNotifications(int customerId);
        List<Notification> GetUnreadNotifications(int customerId);
        bool MarkNotificationAsRead(int notificationId);
    }
} 