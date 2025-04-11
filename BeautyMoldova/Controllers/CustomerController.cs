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
    public class CustomerController : Controller
    {
        private readonly ShopDataContext _db;

        public CustomerController()
        {
            _db = new ShopDataContext();
        }

        // Дашборд личного кабинета
        public async Task<ActionResult> Dashboard()
        {
            var username = User.Identity.Name;
            var customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.Username == username);
            
            if (customer == null)
            {
                return RedirectToAction("Enter", "Profile");
            }
            
            // Получение последних заказов
            var recentPurchases = await _db.Purchases
                .Where(p => p.CustomerId == customer.Id)
                .OrderByDescending(p => p.PurchaseDate)
                .Take(5)
                .ToListAsync();
            
            // Получение товаров из избранного
            var wishlistItems = await _db.WishlistItems
                .Where(w => w.CustomerId == customer.Id)
                .Include(w => w.Product)
                .OrderByDescending(w => w.AddedDate)
                .Take(4)
                .ToListAsync();
            
            // Недавно просмотренные товары (можно сделать через куки или другой механизм)
            // Это заглушка для примера
            var recentlyViewedProducts = await _db.Products
                .Where(p => p.IsAvailable)
                .OrderByDescending(p => p.ViewCount)
                .Take(4)
                .ToListAsync();
            
            ViewBag.Customer = customer;
            ViewBag.RecentPurchases = recentPurchases;
            ViewBag.WishlistItems = wishlistItems;
            ViewBag.RecentlyViewedProducts = recentlyViewedProducts;
            
            return View();
        }
        
        // Список всех заказов пользователя
        public async Task<ActionResult> Purchases()
        {
            var username = User.Identity.Name;
            var customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.Username == username);
            
            if (customer == null)
            {
                return RedirectToAction("Enter", "Profile");
            }
            
            var purchases = await _db.Purchases
                .Where(p => p.CustomerId == customer.Id)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();
            
            return View(purchases);
        }
        
        // Детали конкретного заказа
        public async Task<ActionResult> PurchaseDetails(int id)
        {
            var username = User.Identity.Name;
            var customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.Username == username);
            
            if (customer == null)
            {
                return RedirectToAction("Enter", "Profile");
            }
            
            var purchase = await _db.Purchases
                .Include(p => p.PurchaseItems.Select(pi => pi.Product))
                .FirstOrDefaultAsync(p => p.Id == id && p.CustomerId == customer.Id);
            
            if (purchase == null)
            {
                return HttpNotFound();
            }
            
            return View(purchase);
        }
        
        // Список избранных товаров
        public async Task<ActionResult> Wishlist()
        {
            var username = User.Identity.Name;
            var customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.Username == username);
            
            if (customer == null)
            {
                return RedirectToAction("Enter", "Profile");
            }
            
            var wishlistItems = await _db.WishlistItems
                .Where(w => w.CustomerId == customer.Id)
                .Include(w => w.Product.Manufacturer)
                .OrderByDescending(w => w.AddedDate)
                .ToListAsync();
            
            return View(wishlistItems);
        }
        
        // Добавление товара в избранное (AJAX)
        [HttpPost]
        public async Task<ActionResult> AddToWishlist(int productId)
        {
            var username = User.Identity.Name;
            var customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.Username == username);
            
            if (customer == null)
            {
                return Json(new { success = false, message = "Пользователь не найден" });
            }
            
            // Проверяем, есть ли товар в базе
            var product = await _db.Products.FindAsync(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Товар не найден" });
            }
            
            // Проверяем, не добавлен ли товар уже в избранное
            var existingItem = await _db.WishlistItems
                .FirstOrDefaultAsync(w => w.CustomerId == customer.Id && w.ProductId == productId);
            
            if (existingItem != null)
            {
                return Json(new { success = true, message = "Товар уже в избранном" });
            }
            
            // Добавляем товар в избранное
            var wishlistItem = new WishlistItem
            {
                CustomerId = customer.Id,
                ProductId = productId,
                AddedDate = DateTime.Now
            };
            
            _db.WishlistItems.Add(wishlistItem);
            await _db.SaveChangesAsync();
            
            return Json(new { success = true, message = "Товар добавлен в избранное" });
        }
        
        // Удаление товара из избранного (AJAX)
        [HttpPost]
        public async Task<ActionResult> RemoveFromWishlist(int productId)
        {
            var username = User.Identity.Name;
            var customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.Username == username);
            
            if (customer == null)
            {
                return Json(new { success = false, message = "Пользователь не найден" });
            }
            
            // Находим запись в избранном
            var wishlistItem = await _db.WishlistItems
                .FirstOrDefaultAsync(w => w.CustomerId == customer.Id && w.ProductId == productId);
            
            if (wishlistItem == null)
            {
                return Json(new { success = false, message = "Товар не найден в избранном" });
            }
            
            // Удаляем из избранного
            _db.WishlistItems.Remove(wishlistItem);
            await _db.SaveChangesAsync();
            
            return Json(new { success = true, message = "Товар удален из избранного" });
        }
        
        // Профиль пользователя
        public async Task<ActionResult> Profile()
        {
            var username = User.Identity.Name;
            var customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.Username == username);
            
            if (customer == null)
            {
                return RedirectToAction("Enter", "Profile");
            }
            
            return View(customer);
        }
        
        // Обновление профиля пользователя
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateProfile(Customer model)
        {
            var username = User.Identity.Name;
            var customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.Username == username);
            
            if (customer == null)
            {
                return RedirectToAction("Enter", "Profile");
            }
            
            // Обновляем только допустимые поля
            customer.FirstName = model.FirstName;
            customer.LastName = model.LastName;
            customer.PhoneNumber = model.PhoneNumber;
            customer.ShippingAddress = model.ShippingAddress;
            customer.BillingAddress = model.BillingAddress;
            
            await _db.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Профиль успешно обновлен";
            return RedirectToAction("Profile");
        }
        
        // Отзывы пользователя
        public async Task<ActionResult> Reviews()
        {
            var username = User.Identity.Name;
            var customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.Username == username);
            
            if (customer == null)
            {
                return RedirectToAction("Enter", "Profile");
            }
            
            var reviews = await _db.Reviews
                .Where(r => r.CustomerId == customer.Id)
                .Include(r => r.Product)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
            
            return View(reviews);
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