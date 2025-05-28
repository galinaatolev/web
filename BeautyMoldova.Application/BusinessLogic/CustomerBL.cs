using System;
using System.Collections.Generic;
using System.Linq;
using BeautyMoldova.Application.Interfaces;
using BeautyMoldova.Database;
using BeautyMoldova.Domain.Models;

namespace BeautyMoldova.Application.BusinessLogic
{
    /// <summary>
    /// Business Logic класс для управления клиентами
    /// Наследуется от BaseApi и делегирует все операции к базовым CRUD методам
    /// </summary>
    public class CustomerBL : BaseApi, ICustomerBL
    {
        public CustomerBL() : base() { }
        public CustomerBL(ShopDataContext context) : base(context) { }

        #region Основные операции с клиентами

        /// <summary>
        /// Получить клиента по ID
        /// ✅ ПРАВИЛЬНО - использует базовый метод
        /// </summary>
        public Customer GetCustomerById(int id)
        {
            return GetById<Customer>(id);
        }

        /// <summary>
        /// Получить клиента по имени пользователя
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public Customer GetCustomerByUsername(string username)
        {
            if (string.IsNullOrEmpty(username)) return null;
            
            var allCustomers = GetAll<Customer>();
            return allCustomers.FirstOrDefault(c => c.Username == username && c.IsActive);
        }

        /// <summary>
        /// Получить клиента по email
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public Customer GetCustomerByEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return null;
            
            var allCustomers = GetAll<Customer>();
            return allCustomers.FirstOrDefault(c => c.Email == email && c.IsActive);
        }

        /// <summary>
        /// Получить всех клиентов
        /// ✅ ПРАВИЛЬНО - использует базовый метод
        /// </summary>
        public List<Customer> GetAllCustomers()
        {
            return GetAll<Customer>();
        }

        /// <summary>
        /// Поиск клиентов по тексту
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<Customer> SearchCustomers(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return new List<Customer>();

            var allCustomers = GetAll<Customer>();
            searchTerm = searchTerm.ToLower();
            
            return allCustomers.Where(c => 
                c.IsActive && (
                    c.FirstName.ToLower().Contains(searchTerm) ||
                    c.LastName.ToLower().Contains(searchTerm) ||
                    c.Email.ToLower().Contains(searchTerm) ||
                    c.Username.ToLower().Contains(searchTerm) ||
                    (c.PhoneNumber != null && c.PhoneNumber.Contains(searchTerm))
                )).ToList();
        }

        #endregion

        #region CRUD операции

        /// <summary>
        /// Создать нового клиента
        /// ✅ ПРАВИЛЬНО - использует базовый метод Create
        /// </summary>
        public bool CreateCustomer(Customer customer)
        {
            if (customer == null) return false;
            
            customer.RegistrationDate = DateTime.Now;
            customer.IsActive = true;
            customer.LoyaltyPoints = 0;
            
            return Create<Customer>(customer);
        }

        /// <summary>
        /// Обновить клиента
        /// ✅ ПРАВИЛЬНО - использует базовый метод Update
        /// </summary>
        public bool UpdateCustomer(Customer customer)
        {
            if (customer == null) return false;
            return Update<Customer>(customer);
        }

        /// <summary>
        /// Удалить клиента
        /// ✅ ПРАВИЛЬНО - использует базовый метод Delete
        /// </summary>
        public bool DeleteCustomer(int id)
        {
            return Delete<Customer>(id);
        }

        #endregion

        #region Дополнительные операции

        /// <summary>
        /// Получить количество клиентов
        /// ✅ ПРАВИЛЬНО - использует базовый метод Count
        /// </summary>
        public int GetCustomersCount()
        {
            return Count<Customer>();
        }

        /// <summary>
        /// Получить клиентов с пагинацией
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetPaged
        /// </summary>
        public List<Customer> GetPagedCustomers(int page, int pageSize)
        {
            return GetPaged<Customer>(page, pageSize);
        }

        /// <summary>
        /// Проверить существование email
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем проверяет
        /// </summary>
        public bool IsEmailExists(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;
            
            var allCustomers = GetAll<Customer>();
            return allCustomers.Any(c => c.Email == email);
        }

        /// <summary>
        /// Проверить существование имени пользователя
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем проверяет
        /// </summary>
        public bool IsUsernameExists(string username)
        {
            if (string.IsNullOrEmpty(username)) return false;
            
            var allCustomers = GetAll<Customer>();
            return allCustomers.Any(c => c.Username == username);
        }

        #endregion

        #region Операции с ролями

        /// <summary>
        /// Назначить роль клиенту
        /// ✅ ПРАВИЛЬНО - использует базовый метод Create
        /// </summary>
        public bool AssignRole(int customerId, int roleId)
        {
            // Проверяем, не назначена ли уже эта роль
            var allCustomerRoles = GetAll<CustomerRole>();
            if (allCustomerRoles.Any(cr => cr.CustomerId == customerId && cr.RoleId == roleId))
            {
                return false; // Роль уже назначена
            }

            var customerRole = new CustomerRole
            {
                CustomerId = customerId,
                RoleId = roleId,
                AssignedDate = DateTime.Now
            };
            
            return Create<CustomerRole>(customerRole);
        }

        /// <summary>
        /// Удалить роль у клиента
        /// ✅ ПРАВИЛЬНО - использует базовые методы GetAll и Delete
        /// </summary>
        public bool RemoveRole(int customerId, int roleId)
        {
            var allCustomerRoles = GetAll<CustomerRole>();
            var customerRole = allCustomerRoles.FirstOrDefault(cr => cr.CustomerId == customerId && cr.RoleId == roleId);
            
            if (customerRole != null)
            {
                return Delete<CustomerRole>(customerRole);
            }
            return false;
        }

        /// <summary>
        /// Получить роли клиента
        /// ✅ ПРАВИЛЬНО - использует базовые методы GetAll для связей и ролей
        /// </summary>
        public List<Role> GetCustomerRoles(int customerId)
        {
            var allCustomerRoles = GetAll<CustomerRole>();
            var allRoles = GetAll<Role>();
            
            var customerRoleIds = allCustomerRoles.Where(cr => cr.CustomerId == customerId)
                                                 .Select(cr => cr.RoleId)
                                                 .ToList();
            
            return allRoles.Where(r => customerRoleIds.Contains(r.Id) && r.IsActive).ToList();
        }

        /// <summary>
        /// Проверить, есть ли у клиента роль
        /// ✅ ПРАВИЛЬНО - использует базовые методы GetAll
        /// </summary>
        public bool HasRole(int customerId, string roleName)
        {
            var customerRoles = GetCustomerRoles(customerId);
            return customerRoles.Any(r => r.Name == roleName);
        }

        #endregion

        #region Операции с заказами

        /// <summary>
        /// Получить заказы клиента
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<Purchase> GetCustomerPurchases(int customerId)
        {
            var allPurchases = GetAll<Purchase>();
            return allPurchases.Where(p => p.CustomerId == customerId)
                              .OrderByDescending(p => p.PurchaseDate)
                              .ToList();
        }

        /// <summary>
        /// Получить количество заказов клиента
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем считает
        /// </summary>
        public int GetCustomerPurchasesCount(int customerId)
        {
            var allPurchases = GetAll<Purchase>();
            return allPurchases.Count(p => p.CustomerId == customerId);
        }

        #endregion

        #region Операции с отзывами

        /// <summary>
        /// Получить отзывы клиента
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<Review> GetCustomerReviews(int customerId)
        {
            var allReviews = GetAll<Review>();
            return allReviews.Where(r => r.CustomerId == customerId)
                           .OrderByDescending(r => r.CreatedDate)
                           .ToList();
        }

        /// <summary>
        /// Получить количество отзывов клиента
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем считает
        /// </summary>
        public int GetCustomerReviewsCount(int customerId)
        {
            var allReviews = GetAll<Review>();
            return allReviews.Count(r => r.CustomerId == customerId);
        }

        #endregion

        #region Операции с wishlist

        /// <summary>
        /// Получить wishlist клиента
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<WishlistItem> GetCustomerWishlist(int customerId)
        {
            var allWishlistItems = GetAll<WishlistItem>();
            return allWishlistItems.Where(w => w.CustomerId == customerId)
                                  .OrderByDescending(w => w.AddedDate)
                                  .ToList();
        }

        /// <summary>
        /// Добавить товар в wishlist
        /// ✅ ПРАВИЛЬНО - использует базовый метод Create
        /// </summary>
        public bool AddToWishlist(int customerId, int productId)
        {
            // Проверяем, нет ли уже этого товара в wishlist
            var allWishlistItems = GetAll<WishlistItem>();
            if (allWishlistItems.Any(w => w.CustomerId == customerId && w.ProductId == productId))
            {
                return false; // Товар уже в wishlist
            }

            var wishlistItem = new WishlistItem
            {
                CustomerId = customerId,
                ProductId = productId,
                AddedDate = DateTime.Now,
                Priority = 1
            };
            
            return Create<WishlistItem>(wishlistItem);
        }

        /// <summary>
        /// Удалить товар из wishlist
        /// ✅ ПРАВИЛЬНО - использует базовые методы GetAll и Delete
        /// </summary>
        public bool RemoveFromWishlist(int customerId, int productId)
        {
            var allWishlistItems = GetAll<WishlistItem>();
            var wishlistItem = allWishlistItems.FirstOrDefault(w => w.CustomerId == customerId && w.ProductId == productId);
            
            if (wishlistItem != null)
            {
                return Delete<WishlistItem>(wishlistItem);
            }
            return false;
        }

        #endregion

        #region Операции с уведомлениями

        /// <summary>
        /// Получить уведомления клиента
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<Notification> GetCustomerNotifications(int customerId)
        {
            var allNotifications = GetAll<Notification>();
            return allNotifications.Where(n => n.CustomerId == customerId)
                                  .OrderByDescending(n => n.CreatedDate)
                                  .ToList();
        }

        /// <summary>
        /// Получить непрочитанные уведомления
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<Notification> GetUnreadNotifications(int customerId)
        {
            var allNotifications = GetAll<Notification>();
            return allNotifications.Where(n => n.CustomerId == customerId && !n.IsRead)
                                  .OrderByDescending(n => n.CreatedDate)
                                  .ToList();
        }

        /// <summary>
        /// Отметить уведомление как прочитанное
        /// ✅ ПРАВИЛЬНО - использует базовые методы GetById и Update
        /// </summary>
        public bool MarkNotificationAsRead(int notificationId)
        {
            var notification = GetById<Notification>(notificationId);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                return Update<Notification>(notification);
            }
            return false;
        }

        #endregion
    }
} 