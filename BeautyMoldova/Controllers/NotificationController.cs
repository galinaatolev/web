using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeautyMoldova.Database;
using BeautyMoldova.Domain.Models;

namespace BeautyMoldova.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly ShopDataContext _db;

        public NotificationController()
        {
            _db = new ShopDataContext();
        }
        
        // Список уведомлений пользователя
        public async Task<ActionResult> Index()
        {
            var username = User.Identity.Name;
            var customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.Username == username);
            
            if (customer == null)
            {
                return RedirectToAction("Enter", "Profile");
            }
            
            var notifications = await _db.Notifications
                .Where(n => n.CustomerId == customer.Id)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
            
            return View(notifications);
        }
        
        // Детали уведомления
        public async Task<ActionResult> Details(int id)
        {
            var username = User.Identity.Name;
            var customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.Username == username);
            
            if (customer == null)
            {
                return RedirectToAction("Enter", "Profile");
            }
            
            var notification = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.CustomerId == customer.Id);
            
            if (notification == null)
            {
                return HttpNotFound();
            }
            
            // Отмечаем уведомление как прочитанное
            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadDate = DateTime.Now;
                await _db.SaveChangesAsync();
            }
            
            return View(notification);
        }
        
        // AJAX метод для получения новых уведомлений
        [HttpGet]
        public async Task<JsonResult> GetNewNotifications()
        {
            var username = User.Identity.Name;
            var customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.Username == username);
            
            if (customer == null)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            
            var newNotifications = await _db.Notifications
                .Where(n => n.CustomerId == customer.Id && !n.IsRead)
                .OrderByDescending(n => n.CreatedDate)
                .Select(n => new {
                    n.Id,
                    n.Title,
                    n.Message,
                    n.Type,
                    CreatedDate = n.CreatedDate.ToString("dd.MM.yyyy HH:mm")
                })
                .ToListAsync();
            
            return Json(new { 
                success = true, 
                count = newNotifications.Count,
                notifications = newNotifications 
            }, JsonRequestBehavior.AllowGet);
        }
        
        // Отметить все уведомления как прочитанные
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MarkAllAsRead()
        {
            var username = User.Identity.Name;
            var customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.Username == username);
            
            if (customer == null)
            {
                return RedirectToAction("Enter", "Profile");
            }
            
            var unreadNotifications = await _db.Notifications
                .Where(n => n.CustomerId == customer.Id && !n.IsRead)
                .ToListAsync();
            
            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadDate = DateTime.Now;
            }
            
            await _db.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Все уведомления отмечены как прочитанные";
            return RedirectToAction("Index");
        }
        
        // Удаление уведомления
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            var username = User.Identity.Name;
            var customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.Username == username);
            
            if (customer == null)
            {
                return RedirectToAction("Enter", "Profile");
            }
            
            var notification = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.CustomerId == customer.Id);
            
            if (notification == null)
            {
                return HttpNotFound();
            }
            
            _db.Notifications.Remove(notification);
            await _db.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Уведомление удалено";
            return RedirectToAction("Index");
        }
        
        // Удаление всех уведомлений
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteAll()
        {
            var username = User.Identity.Name;
            var customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.Username == username);
            
            if (customer == null)
            {
                return RedirectToAction("Enter", "Profile");
            }
            
            var notifications = await _db.Notifications
                .Where(n => n.CustomerId == customer.Id)
                .ToListAsync();
            
            _db.Notifications.RemoveRange(notifications);
            await _db.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Все уведомления удалены";
            return RedirectToAction("Index");
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
} 