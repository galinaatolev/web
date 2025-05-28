using System;
using System.Collections.Generic;
using System.Linq;
using BeautyMoldova.Application.Interfaces;
using BeautyMoldova.Database;
using BeautyMoldova.Domain.Models;

namespace BeautyMoldova.Application.BusinessLogic
{
    /// <summary>
    /// Business Logic класс для управления уведомлениями
    /// Наследуется от BaseApi и делегирует все операции к базовым CRUD методам
    /// </summary>
    public class NotificationBL : BaseApi, INotificationBL
    {
        public NotificationBL() : base() { }
        public NotificationBL(ShopDataContext context) : base(context) { }

        #region Основные операции с уведомлениями

        /// <summary>
        /// Получить уведомление по ID
        /// ✅ ПРАВИЛЬНО - использует базовый метод
        /// </summary>
        public Notification GetNotificationById(int id)
        {
            return GetById<Notification>(id);
        }

        /// <summary>
        /// Получить все уведомления
        /// ✅ ПРАВИЛЬНО - использует базовый метод
        /// </summary>
        public List<Notification> GetAllNotifications()
        {
            return GetAll<Notification>();
        }

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
        /// Получить непрочитанные уведомления клиента
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<Notification> GetUnreadNotifications(int customerId)
        {
            var allNotifications = GetAll<Notification>();
            return allNotifications.Where(n => n.CustomerId == customerId && !n.IsRead)
                                  .OrderByDescending(n => n.CreatedDate)
                                  .ToList();
        }

        #endregion

        #region CRUD операции

        /// <summary>
        /// Создать новое уведомление
        /// ✅ ПРАВИЛЬНО - использует базовый метод Create
        /// </summary>
        public bool CreateNotification(Notification notification)
        {
            if (notification == null) return false;
            
            notification.CreatedDate = DateTime.Now;
            notification.IsRead = false;
            
            return Create<Notification>(notification);
        }

        /// <summary>
        /// Обновить уведомление
        /// ✅ ПРАВИЛЬНО - использует базовый метод Update
        /// </summary>
        public bool UpdateNotification(Notification notification)
        {
            if (notification == null) return false;
            
            return Update<Notification>(notification);
        }

        /// <summary>
        /// Удалить уведомление
        /// ✅ ПРАВИЛЬНО - использует базовый метод Delete
        /// </summary>
        public bool DeleteNotification(int id)
        {
            return Delete<Notification>(id);
        }

        #endregion

        #region Дополнительные операции

        /// <summary>
        /// Получить количество уведомлений
        /// ✅ ПРАВИЛЬНО - использует базовый метод Count
        /// </summary>
        public int GetNotificationsCount()
        {
            return Count<Notification>();
        }

        /// <summary>
        /// Получить количество непрочитанных уведомлений клиента
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public int GetUnreadNotificationsCount(int customerId)
        {
            return GetUnreadNotifications(customerId).Count;
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
                notification.ReadDate = DateTime.Now;
                return Update<Notification>(notification);
            }
            return false;
        }

        /// <summary>
        /// Отметить все уведомления клиента как прочитанные
        /// ✅ ПРАВИЛЬНО - использует базовые методы GetAll и Update
        /// </summary>
        public bool MarkAllNotificationsAsRead(int customerId)
        {
            var unreadNotifications = GetUnreadNotifications(customerId);
            bool success = true;
            
            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadDate = DateTime.Now;
                if (!Update<Notification>(notification))
                {
                    success = false;
                }
            }
            
            return success;
        }

        /// <summary>
        /// Удалить все уведомления клиента
        /// ✅ ПРАВИЛЬНО - использует базовые методы GetAll и Delete
        /// </summary>
        public bool DeleteAllNotifications(int customerId)
        {
            var customerNotifications = GetCustomerNotifications(customerId);
            bool success = true;
            
            foreach (var notification in customerNotifications)
            {
                if (!Delete<Notification>(notification.Id))
                {
                    success = false;
                }
            }
            
            return success;
        }

        /// <summary>
        /// Проверить принадлежность уведомления пользователю
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetById
        /// </summary>
        public bool IsNotificationOwnedByCustomer(int notificationId, int customerId)
        {
            var notification = GetById<Notification>(notificationId);
            return notification != null && notification.CustomerId == customerId;
        }

        #endregion
    }
} 