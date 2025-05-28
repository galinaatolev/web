using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeautyMoldova.Application.Interfaces;
using BeautyMoldova.Application.BusinessLogic;
using BeautyMoldova.Domain.Models;

namespace BeautyMoldova.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        // ✅ ПРАВИЛЬНО - используем Business Logic вместо прямого контекста
        private readonly INotificationBL _notificationBL;
        private readonly ICustomerBL _customerBL;

        public NotificationController()
        {
            _notificationBL = new NotificationBL();
            _customerBL = new CustomerBL();
        }
        
        // Список уведомлений пользователя
        public ActionResult Index()
        {
            var username = User.Identity.Name;
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var customer = _customerBL.GetCustomerByUsername(username);
            
            if (customer == null)
            {
                return RedirectToAction("Enter", "Profile");
            }
            
            var notifications = _notificationBL.GetCustomerNotifications(customer.Id);
            
            return View(notifications);
        }
        
        // Детали уведомления
        public ActionResult Details(int id)
        {
            var username = User.Identity.Name;
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var customer = _customerBL.GetCustomerByUsername(username);
            
            if (customer == null)
            {
                return RedirectToAction("Enter", "Profile");
            }
            
            var notification = _notificationBL.GetNotificationById(id);
            
            if (notification == null || notification.CustomerId != customer.Id)
            {
                return HttpNotFound();
            }
            
            // Отмечаем уведомление как прочитанное
            if (!notification.IsRead)
            {
                _notificationBL.MarkNotificationAsRead(id);
            }
            
            return View(notification);
        }
        
        // AJAX метод для получения новых уведомлений
        [HttpGet]
        public JsonResult GetNewNotifications()
        {
            var username = User.Identity.Name;
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var customer = _customerBL.GetCustomerByUsername(username);
            
            if (customer == null)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            
            var newNotifications = _notificationBL.GetUnreadNotifications(customer.Id)
                .Select(n => new {
                    n.Id,
                    n.Title,
                    n.Message,
                    n.Type,
                    CreatedDate = n.CreatedDate.ToString("dd.MM.yyyy HH:mm")
                })
                .ToList();
            
            return Json(new { 
                success = true, 
                count = newNotifications.Count,
                notifications = newNotifications 
            }, JsonRequestBehavior.AllowGet);
        }
        
        // Отметить все уведомления как прочитанные
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkAllAsRead()
        {
            var username = User.Identity.Name;
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var customer = _customerBL.GetCustomerByUsername(username);
            
            if (customer == null)
            {
                return RedirectToAction("Enter", "Profile");
            }
            
            if (_notificationBL.MarkAllNotificationsAsRead(customer.Id))
            {
                TempData["SuccessMessage"] = "Все уведомления отмечены как прочитанные";
            }
            else
            {
                TempData["ErrorMessage"] = "Ошибка при обновлении уведомлений";
            }
            
            return RedirectToAction("Index");
        }
        
        // Удаление уведомления
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var username = User.Identity.Name;
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var customer = _customerBL.GetCustomerByUsername(username);
            
            if (customer == null)
            {
                return RedirectToAction("Enter", "Profile");
            }
            
            // Проверяем принадлежность уведомления пользователю
            if (!_notificationBL.IsNotificationOwnedByCustomer(id, customer.Id))
            {
                return HttpNotFound();
            }
            
            if (_notificationBL.DeleteNotification(id))
            {
                TempData["SuccessMessage"] = "Уведомление удалено";
            }
            else
            {
                TempData["ErrorMessage"] = "Ошибка при удалении уведомления";
            }
            
            return RedirectToAction("Index");
        }
        
        // Удаление всех уведомлений
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAll()
        {
            var username = User.Identity.Name;
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var customer = _customerBL.GetCustomerByUsername(username);
            
            if (customer == null)
            {
                return RedirectToAction("Enter", "Profile");
            }
            
            if (_notificationBL.DeleteAllNotifications(customer.Id))
            {
                TempData["SuccessMessage"] = "Все уведомления удалены";
            }
            else
            {
                TempData["ErrorMessage"] = "Ошибка при удалении уведомлений";
            }
            
            return RedirectToAction("Index");
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                (_notificationBL as IDisposable)?.Dispose();
                (_customerBL as IDisposable)?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
} 