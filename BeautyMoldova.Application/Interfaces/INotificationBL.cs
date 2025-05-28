using System.Collections.Generic;
using BeautyMoldova.Domain.Models;

namespace BeautyMoldova.Application.Interfaces
{
    public interface INotificationBL
    {
        // Основные операции с уведомлениями
        Notification GetNotificationById(int id);
        List<Notification> GetAllNotifications();
        List<Notification> GetCustomerNotifications(int customerId);
        List<Notification> GetUnreadNotifications(int customerId);
        
        // CRUD операции
        bool CreateNotification(Notification notification);
        bool UpdateNotification(Notification notification);
        bool DeleteNotification(int id);
        
        // Дополнительные операции
        int GetNotificationsCount();
        int GetUnreadNotificationsCount(int customerId);
        bool MarkNotificationAsRead(int notificationId);
        bool MarkAllNotificationsAsRead(int customerId);
        bool DeleteAllNotifications(int customerId);
        
        // Проверка принадлежности уведомления пользователю
        bool IsNotificationOwnedByCustomer(int notificationId, int customerId);
    }
} 