using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeautyMoldova.Application.Interfaces;
using BeautyMoldova.Application.BusinessLogic;
using BeautyMoldova.Domain.Models;

namespace BeautyMoldova.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        // ✅ ПРАВИЛЬНО - используем Business Logic вместо прямого контекста
        private readonly ICustomerBL _customerBL;
        private readonly IPurchaseBL _purchaseBL;

        public CustomerController()
        {
            _customerBL = new CustomerBL();
            _purchaseBL = new PurchaseBL();
        }

        // Дашборд личного кабинета
        public ActionResult Dashboard()
        {
            var username = User.Identity.Name;
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var customer = _customerBL.GetCustomerByUsername(username);
            
            if (customer == null)
            {
                return RedirectToAction("Enter", "Profile");
            }
            
            // Получение последних заказов
            var recentPurchases = _purchaseBL.GetPurchasesByCustomer(customer.Id).Take(5).ToList();
            
            // Получение товаров из избранного
            var wishlistItems = _customerBL.GetCustomerWishlist(customer.Id).Take(4).ToList();
            
            ViewBag.Customer = customer;
            ViewBag.RecentPurchases = recentPurchases;
            ViewBag.WishlistItems = wishlistItems;
            ViewBag.TotalOrders = _purchaseBL.GetPurchasesByCustomer(customer.Id).Count;
            ViewBag.UnreadNotifications = _customerBL.GetUnreadNotifications(customer.Id).Count;
            
            return View();
        }
        
        // Список заказов клиента
        public ActionResult Orders(int page = 1)
        {
            var username = User.Identity.Name;
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var customer = _customerBL.GetCustomerByUsername(username);
            
            if (customer == null)
            {
                return RedirectToAction("Enter", "Profile");
            }
            
            var orders = _purchaseBL.GetPurchasesByCustomer(customer.Id);
            
            // Пагинация
            const int pageSize = 10;
            var totalOrders = orders.Count;
            var pagedOrders = orders.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
            ViewBag.TotalOrders = totalOrders;
            
            return View(pagedOrders);
        }
        
        // Детали заказа
        public ActionResult OrderDetails(int id)
        {
            var username = User.Identity.Name;
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var customer = _customerBL.GetCustomerByUsername(username);
            
            if (customer == null)
            {
                return RedirectToAction("Enter", "Profile");
            }
            
            var order = _purchaseBL.GetPurchaseById(id);
            
            if (order == null || order.CustomerId != customer.Id)
            {
                return HttpNotFound();
            }
            
            return View(order);
        }
        
        // Список избранного
        public ActionResult Wishlist()
        {
            var username = User.Identity.Name;
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var customer = _customerBL.GetCustomerByUsername(username);
            
            if (customer == null)
            {
                return RedirectToAction("Enter", "Profile");
            }
            
            var wishlistItems = _customerBL.GetCustomerWishlist(customer.Id);
            
            return View(wishlistItems);
        }
        
        // Добавление в избранное
        [HttpPost]
        public ActionResult AddToWishlist(int productId)
        {
            var username = User.Identity.Name;
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var customer = _customerBL.GetCustomerByUsername(username);
            
            if (customer == null)
            {
                return Json(new { success = false, message = "Пользователь не найден" });
            }
            
            if (_customerBL.AddToWishlist(customer.Id, productId))
            {
                return Json(new { success = true, message = "Товар добавлен в избранное" });
            }
            else
            {
                return Json(new { success = false, message = "Товар уже в избранном" });
            }
        }
        
        // Удаление из избранного
        [HttpPost]
        public ActionResult RemoveFromWishlist(int productId)
        {
            var username = User.Identity.Name;
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var customer = _customerBL.GetCustomerByUsername(username);
            
            if (customer == null)
            {
                return Json(new { success = false, message = "Пользователь не найден" });
            }
            
            if (_customerBL.RemoveFromWishlist(customer.Id, productId))
            {
                return Json(new { success = true, message = "Товар удален из избранного" });
            }
            else
            {
                return Json(new { success = false, message = "Ошибка при удалении" });
            }
        }
        
        // Уведомления
        public ActionResult Notifications()
        {
            var username = User.Identity.Name;
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var customer = _customerBL.GetCustomerByUsername(username);
            
            if (customer == null)
            {
                return RedirectToAction("Enter", "Profile");
            }
            
            var notifications = _customerBL.GetCustomerNotifications(customer.Id);
            
            return View(notifications);
        }
        
        // Отметить уведомление как прочитанное
        [HttpPost]
        public ActionResult MarkNotificationAsRead(int notificationId)
        {
            // ✅ ПРАВИЛЬНО - используем Business Logic
            if (_customerBL.MarkNotificationAsRead(notificationId))
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false });
            }
        }
        
        // Профиль пользователя
        public ActionResult Profile()
        {
            var username = User.Identity.Name;
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var customer = _customerBL.GetCustomerByUsername(username);
            
            if (customer == null)
            {
                return RedirectToAction("Enter", "Profile");
            }
            
            return View(customer);
        }
        
        // Редактирование профиля
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(Customer model)
        {
            var username = User.Identity.Name;
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var customer = _customerBL.GetCustomerByUsername(username);
            
            if (customer == null)
            {
                return RedirectToAction("Enter", "Profile");
            }
            
            // Обновляем только разрешенные поля
            customer.FirstName = model.FirstName;
            customer.LastName = model.LastName;
            customer.Email = model.Email;
            customer.PhoneNumber = model.PhoneNumber;
            customer.DateOfBirth = model.DateOfBirth;
            customer.ShippingAddress = model.ShippingAddress;
            customer.BillingAddress = model.BillingAddress;
            
            if (_customerBL.UpdateCustomer(customer))
            {
                TempData["SuccessMessage"] = "Профиль успешно обновлен";
            }
            else
            {
                TempData["ErrorMessage"] = "Ошибка при обновлении профиля";
            }
            
            return RedirectToAction("Profile");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                (_customerBL as IDisposable)?.Dispose();
                (_purchaseBL as IDisposable)?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
} 