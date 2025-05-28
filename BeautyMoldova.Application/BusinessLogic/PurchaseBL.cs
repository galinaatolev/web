using System;
using System.Collections.Generic;
using System.Linq;
using BeautyMoldova.Application.Interfaces;
using BeautyMoldova.Database;
using BeautyMoldova.Domain.Models;

namespace BeautyMoldova.Application.BusinessLogic
{
    /// <summary>
    /// Business Logic класс для управления заказами
    /// Наследуется от BaseApi и делегирует все операции к базовым CRUD методам
    /// </summary>
    public class PurchaseBL : BaseApi, IPurchaseBL
    {
        public PurchaseBL() : base() { }
        public PurchaseBL(ShopDataContext context) : base(context) { }

        #region Основные операции с заказами

        /// <summary>
        /// Получить заказ по ID
        /// ✅ ПРАВИЛЬНО - использует базовый метод
        /// </summary>
        public Purchase GetPurchaseById(int id)
        {
            return GetById<Purchase>(id);
        }

        /// <summary>
        /// Получить все заказы
        /// ✅ ПРАВИЛЬНО - использует базовый метод
        /// </summary>
        public List<Purchase> GetAllPurchases()
        {
            return GetAll<Purchase>();
        }

        /// <summary>
        /// Получить заказы клиента
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<Purchase> GetPurchasesByCustomer(int customerId)
        {
            var allPurchases = GetAll<Purchase>();
            return allPurchases.Where(p => p.CustomerId == customerId)
                              .OrderByDescending(p => p.PurchaseDate)
                              .ToList();
        }

        /// <summary>
        /// Получить заказы по статусу
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<Purchase> GetPurchasesByStatus(string status)
        {
            var allPurchases = GetAll<Purchase>();
            return allPurchases.Where(p => p.Status == status)
                              .OrderByDescending(p => p.PurchaseDate)
                              .ToList();
        }

        /// <summary>
        /// Поиск заказов по тексту
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<Purchase> SearchPurchases(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return new List<Purchase>();

            var allPurchases = GetAll<Purchase>();
            var allCustomers = GetAll<Customer>();
            searchTerm = searchTerm.ToLower();
            
            return allPurchases.Where(p => 
                p.Id.ToString().Contains(searchTerm) ||
                p.OrderNumber.ToLower().Contains(searchTerm) ||
                allCustomers.Any(c => c.Id == p.CustomerId && 
                    (c.FirstName.ToLower().Contains(searchTerm) ||
                     c.LastName.ToLower().Contains(searchTerm) ||
                     c.Email.ToLower().Contains(searchTerm)))
            ).OrderByDescending(p => p.PurchaseDate).ToList();
        }

        #endregion

        #region CRUD операции

        /// <summary>
        /// Создать новый заказ
        /// ✅ ПРАВИЛЬНО - использует базовый метод Create
        /// </summary>
        public bool CreatePurchase(Purchase purchase)
        {
            if (purchase == null) return false;
            
            purchase.OrderDate = DateTime.Now;
            purchase.PurchaseDate = DateTime.Now;
            purchase.Status = "Pending";
            
            if (string.IsNullOrEmpty(purchase.OrderNumber))
            {
                purchase.OrderNumber = "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            }
            
            return Create<Purchase>(purchase);
        }

        /// <summary>
        /// Обновить заказ
        /// ✅ ПРАВИЛЬНО - использует базовый метод Update
        /// </summary>
        public bool UpdatePurchase(Purchase purchase)
        {
            if (purchase == null) return false;
            return Update<Purchase>(purchase);
        }

        /// <summary>
        /// Удалить заказ
        /// ✅ ПРАВИЛЬНО - использует базовый метод Delete
        /// </summary>
        public bool DeletePurchase(int id)
        {
            return Delete<Purchase>(id);
        }

        #endregion

        #region Дополнительные операции

        /// <summary>
        /// Получить количество заказов
        /// ✅ ПРАВИЛЬНО - использует базовый метод Count
        /// </summary>
        public int GetPurchasesCount()
        {
            return Count<Purchase>();
        }

        /// <summary>
        /// Получить заказы с пагинацией
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetPaged
        /// </summary>
        public List<Purchase> GetPagedPurchases(int page, int pageSize)
        {
            return GetPaged<Purchase>(page, pageSize);
        }

        /// <summary>
        /// Получить заказы за период
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<Purchase> GetPurchasesByDateRange(DateTime startDate, DateTime endDate)
        {
            var allPurchases = GetAll<Purchase>();
            return allPurchases.Where(p => p.PurchaseDate >= startDate && p.PurchaseDate <= endDate)
                              .OrderByDescending(p => p.PurchaseDate)
                              .ToList();
        }

        #endregion

        #region Операции со статусом заказа

        /// <summary>
        /// Обновить статус заказа
        /// ✅ ПРАВИЛЬНО - использует базовые методы GetById и Update
        /// </summary>
        public bool UpdatePurchaseStatus(int purchaseId, string newStatus)
        {
            var purchase = GetById<Purchase>(purchaseId);
            if (purchase != null)
            {
                purchase.Status = newStatus;
                
                // Устанавливаем даты в зависимости от статуса
                switch (newStatus)
                {
                    case "Shipped":
                        purchase.ShippingDate = DateTime.Now;
                        break;
                    case "Delivered":
                        purchase.DeliveryDate = DateTime.Now;
                        break;
                }
                
                return Update<Purchase>(purchase);
            }
            return false;
        }

        /// <summary>
        /// Отменить заказ
        /// ✅ ПРАВИЛЬНО - использует базовые методы GetById и Update
        /// </summary>
        public bool CancelPurchase(int purchaseId, string reason)
        {
            var purchase = GetById<Purchase>(purchaseId);
            if (purchase != null && purchase.Status != "Delivered")
            {
                purchase.Status = "Cancelled";
                purchase.AdminNotes = reason;
                
                return Update<Purchase>(purchase);
            }
            return false;
        }

        #endregion

        #region Операции с элементами заказа

        /// <summary>
        /// Получить элементы заказа
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<PurchaseItem> GetPurchaseItems(int purchaseId)
        {
            var allPurchaseItems = GetAll<PurchaseItem>();
            return allPurchaseItems.Where(pi => pi.PurchaseId == purchaseId).ToList();
        }

        /// <summary>
        /// Добавить элемент заказа
        /// ✅ ПРАВИЛЬНО - использует базовый метод Create
        /// </summary>
        public bool AddPurchaseItem(PurchaseItem item)
        {
            if (item == null) return false;
            return Create<PurchaseItem>(item);
        }

        /// <summary>
        /// Удалить элемент заказа
        /// ✅ ПРАВИЛЬНО - использует базовый метод Delete
        /// </summary>
        public bool RemovePurchaseItem(int itemId)
        {
            return Delete<PurchaseItem>(itemId);
        }

        #endregion

        #region Аналитика

        /// <summary>
        /// Получить общую выручку
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем вычисляет
        /// </summary>
        public decimal GetTotalRevenue()
        {
            var allPurchases = GetAll<Purchase>();
            return allPurchases.Where(p => p.Status != "Cancelled")
                              .Sum(p => p.TotalAmount);
        }

        /// <summary>
        /// Получить выручку за период
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует и вычисляет
        /// </summary>
        public decimal GetRevenueByDateRange(DateTime startDate, DateTime endDate)
        {
            var allPurchases = GetAll<Purchase>();
            return allPurchases.Where(p => p.Status != "Cancelled" && 
                                          p.PurchaseDate >= startDate && 
                                          p.PurchaseDate <= endDate)
                              .Sum(p => p.TotalAmount);
        }

        /// <summary>
        /// Получить последние заказы
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем сортирует и берет нужное количество
        /// </summary>
        public List<Purchase> GetRecentPurchases(int count)
        {
            var allPurchases = GetAll<Purchase>();
            return allPurchases.OrderByDescending(p => p.PurchaseDate)
                              .Take(count)
                              .ToList();
        }

        #endregion
    }
} 