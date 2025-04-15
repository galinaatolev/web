using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BeautyMoldova.Database;
using BeautyMoldova.Domain.Models;
using Newtonsoft.Json;

namespace BeautyMoldova.Controllers
{
    public class CartController : Controller
    {
        private readonly ShopDataContext _db;
        private const string CartSessionKey = "CartItems";

        public CartController()
        {
            _db = new ShopDataContext();
        }
        
        // Отображение страницы корзины
        public async Task<ActionResult> Index()
        {
            var cartItems = GetCartItems();
            
            if (cartItems.Count == 0)
            {
                return View(new List<CartItemViewModel>());
            }
            
            var productIds = cartItems.Select(i => i.ProductId).ToList();
            var products = await _db.Products
                .Include(p => p.Manufacturer)
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();
            
            var cartViewModels = new List<CartItemViewModel>();
            
            foreach (var item in cartItems)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    cartViewModels.Add(new CartItemViewModel
                    {
                        ProductId = product.Id,
                        Name = product.Name,
                        Manufacturer = product.Manufacturer.Name,
                        Price = product.DiscountPrice ?? product.Price,
                        Quantity = item.Quantity,
                        ImageUrl = product.MainImage,
                        StockQuantity = product.StockQuantity,
                        SubTotal = (product.DiscountPrice ?? product.Price) * item.Quantity
                    });
                }
            }
            
            return View(cartViewModels);
        }
        
        // Добавление товара в корзину
        [HttpPost]
        public async Task<ActionResult> AddToCart(int productId, int quantity = 1)
        {
            if (quantity <= 0)
            {
                quantity = 1;
            }
            
            var product = await _db.Products.FindAsync(productId);
            if (product == null || !product.IsAvailable)
            {
                return Json(new { success = false, message = "Product is not available." });
            }
            
            if (quantity > product.StockQuantity)
            {
                return Json(new { success = false, message = $"Only {product.StockQuantity} items available." });
            }
            
            var cartItems = GetCartItems();
            var existingItem = cartItems.FirstOrDefault(i => i.ProductId == productId);
            
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                
                // Проверка на доступное количество
                if (existingItem.Quantity > product.StockQuantity)
                {
                    existingItem.Quantity = product.StockQuantity;
                    SaveCartItems(cartItems);
                    return Json(new { 
                        success = true, 
                        message = $"Maximum available quantity reached ({product.StockQuantity} items).", 
                        cartCount = GetCartCount() 
                    });
                }
            }
            else
            {
                cartItems.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity
                });
            }
            
            SaveCartItems(cartItems);
            
            return Json(new { success = true, message = "Product added to cart.", cartCount = GetCartCount() });
        }
        
        // Обновление количества товара в корзине
        [HttpPost]
        public async Task<ActionResult> UpdateQuantity(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                return Json(new { success = false, message = "Quantity must be greater than 0." });
            }
            
            var product = await _db.Products.FindAsync(productId);
            if (product == null || !product.IsAvailable)
            {
                return Json(new { success = false, message = "Product is not available." });
            }
            
            if (quantity > product.StockQuantity)
            {
                return Json(new { 
                    success = false, 
                    message = $"Only {product.StockQuantity} items available.",
                    maxQuantity = product.StockQuantity
                });
            }
            
            var cartItems = GetCartItems();
            var existingItem = cartItems.FirstOrDefault(i => i.ProductId == productId);
            
            if (existingItem == null)
            {
                return Json(new { success = false, message = "Product not found in cart." });
            }
            
            existingItem.Quantity = quantity;
            SaveCartItems(cartItems);
            
            decimal subtotal = (product.DiscountPrice ?? product.Price) * quantity;
            
            return Json(new { 
                success = true, 
                message = "Quantity updated.", 
                subtotal = subtotal, 
                total = GetCartTotal(),
                cartCount = GetCartCount()
            });
        }
        
        // Удаление товара из корзины
        [HttpPost]
        public ActionResult RemoveFromCart(int productId)
        {
            var cartItems = GetCartItems();
            var itemToRemove = cartItems.FirstOrDefault(i => i.ProductId == productId);
            
            if (itemToRemove != null)
            {
                cartItems.Remove(itemToRemove);
                SaveCartItems(cartItems);
            }
            
            return Json(new { success = true, message = "Product removed from cart.", cartCount = GetCartCount(), total = GetCartTotal() });
        }
        
        // Очистка корзины
        [HttpPost]
        public ActionResult ClearCart()
        {
            Session[CartSessionKey] = null;
            return Json(new { success = true, message = "Cart cleared." });
        }
        
        // Добавление нескольких товаров в корзину сразу
        [HttpPost]
        public async Task<ActionResult> AddToCartBulk(List<CartItemJS> cartItemsJS)
        {
            if (cartItemsJS == null || !cartItemsJS.Any())
            {
                return Json(new { success = false, message = "No items to add to cart." });
            }
            
            // Получаем список ID продуктов из JS корзины
            var productIds = cartItemsJS.Select(i => i.id).ToList();
            
            // Загружаем информацию о продуктах из БД
            var products = await _db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();
            
            // Проверяем наличие товаров
            foreach (var item in cartItemsJS)
            {
                var product = products.FirstOrDefault(p => p.Id == item.id);
                if (product == null || !product.IsAvailable)
                {
                    return Json(new { success = false, message = $"Product with ID {item.id} is not available." });
                }
                
                if (item.qty > product.StockQuantity)
                {
                    return Json(new { 
                        success = false, 
                        message = $"Only {product.StockQuantity} items of {product.Name} are available."
                    });
                }
            }
            
            // Создаем новую серверную корзину
            var cartItems = new List<CartItem>();
            
            // Преобразовываем товары из JSON в формат серверной корзины
            foreach (var item in cartItemsJS)
            {
                cartItems.Add(new CartItem
                {
                    ProductId = item.id,
                    Quantity = item.qty
                });
            }
            
            // Сохраняем корзину в сессии
            SaveCartItems(cartItems);
            
            return Json(new { 
                success = true, 
                message = "Cart items added successfully.", 
                cartCount = GetCartCount() 
            });
        }
        
        // Страница оформления заказа
        [Authorize]
        public async Task<ActionResult> Checkout()
        {
            var cartItems = GetCartItems();
            if (cartItems.Count == 0)
            {
                return RedirectToAction("Index");
            }
            
            var username = User.Identity.Name;
            var customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.Username == username);
            
            if (customer == null)
            {
                return RedirectToAction("Enter", "Profile");
            }
            
            var checkoutViewModel = new CheckoutViewModel
            {
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                ShippingAddress = customer.ShippingAddress,
                BillingAddress = customer.BillingAddress
            };
            
            var productIds = cartItems.Select(i => i.ProductId).ToList();
            var products = await _db.Products
                .Include(p => p.Manufacturer)
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();
            
            var cartViewModels = new List<CartItemViewModel>();
            
            foreach (var item in cartItems)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    cartViewModels.Add(new CartItemViewModel
                    {
                        ProductId = product.Id,
                        Name = product.Name,
                        Manufacturer = product.Manufacturer.Name,
                        Price = product.DiscountPrice ?? product.Price,
                        Quantity = item.Quantity,
                        ImageUrl = product.MainImage,
                        SubTotal = (product.DiscountPrice ?? product.Price) * item.Quantity
                    });
                }
            }
            
            ViewBag.CartItems = cartViewModels;
            ViewBag.CartTotal = cartViewModels.Sum(i => i.SubTotal);
            
            return View(checkoutViewModel);
        }
        
        // Размещение заказа
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PlaceOrder(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var cartItemsList = GetCartItems();
                var productsIds = cartItemsList.Select(i => i.ProductId).ToList();
                var productsItems = await _db.Products
                    .Include(p => p.Manufacturer)
                    .Where(p => productsIds.Contains(p.Id))
                    .ToListAsync();
                
                var cartViewModels = new List<CartItemViewModel>();
                
                foreach (var item in cartItemsList)
                {
                    var product = productsItems.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product != null)
                    {
                        cartViewModels.Add(new CartItemViewModel
                        {
                            ProductId = product.Id,
                            Name = product.Name,
                            Manufacturer = product.Manufacturer.Name,
                            Price = product.DiscountPrice ?? product.Price,
                            Quantity = item.Quantity,
                            ImageUrl = product.MainImage,
                            SubTotal = (product.DiscountPrice ?? product.Price) * item.Quantity
                        });
                    }
                }
                
                ViewBag.CartItems = cartViewModels;
                ViewBag.CartTotal = cartViewModels.Sum(i => i.SubTotal);
                
                return View("Checkout", model);
            }
            
            var cartItems = GetCartItems();
            if (cartItems.Count == 0)
            {
                return RedirectToAction("Index");
            }
            
            var username = User.Identity.Name;
            var customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.Username == username);
            
            if (customer == null)
            {
                return RedirectToAction("Enter", "Profile");
            }
            
            // Проверяем наличие товаров
            var productIds = cartItems.Select(i => i.ProductId).ToList();
            var products = await _db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();
            
            foreach (var item in cartItems)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product == null || !product.IsAvailable || product.StockQuantity < item.Quantity)
                {
                    TempData["ErrorMessage"] = "Some products are unavailable or their quantity has changed. Please check your cart.";
                    return RedirectToAction("Index");
                }
            }
            
            // Создаем заказ
            var purchase = new Purchase
            {
                CustomerId = customer.Id,
                PurchaseDate = DateTime.Now,
                OrderDate = DateTime.Now,
                ShippingAddress = model.ShippingAddress,
                BillingAddress = model.BillingAddress,
                PaymentMethod = model.PaymentMethod,
                ShippingMethod = model.ShippingMethod,
                Status = "Processing",
                Notes = model.Notes,
                PurchaseItems = new List<PurchaseItem>()
            };
            
            // Добавляем товары в заказ
            decimal totalAmount = 0;
            
            foreach (var item in cartItems)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    var price = product.DiscountPrice ?? product.Price;
                    var purchaseItem = new PurchaseItem
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = price,
                        TotalPrice = price * item.Quantity
                    };
                    
                    // Уменьшаем количество товара в наличии
                    product.StockQuantity -= item.Quantity;
                    
                    totalAmount += purchaseItem.TotalPrice;
                    purchase.PurchaseItems.Add(purchaseItem);
                }
            }
            
            purchase.TotalAmount = totalAmount;
            
            // Начисляем бонусные баллы (примерно 5% от суммы заказа)
            int bonusPoints = (int)(totalAmount * 0.05m);
            customer.LoyaltyPoints += bonusPoints;
            
            // Сохраняем изменения в базе данных
            _db.Purchases.Add(purchase);
            await _db.SaveChangesAsync();
            
            // Очищаем корзину
            Session[CartSessionKey] = null;
            
            // Возвращаем пользователя на страницу с подтверждением заказа
            TempData["SuccessMessage"] = "Your order has been successfully placed! Order number: " + purchase.Id;
            return RedirectToAction("OrderConfirmation", new { id = purchase.Id });
        }
        
        // Страница подтверждения заказа
        [Authorize]
        public async Task<ActionResult> OrderConfirmation(int id)
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
        
        // Получение количества товаров в корзине
        [ChildActionOnly]
        public ActionResult CartSummary()
        {
            ViewBag.CartCount = GetCartCount();
            return PartialView("_CartSummary");
        }
        
        // Методы для работы с корзиной в сессии
        private List<CartItem> GetCartItems()
        {
            var cartJson = Session[CartSessionKey] as string;
            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CartItem>();
            }
            
            return JsonConvert.DeserializeObject<List<CartItem>>(cartJson);
        }
        
        private void SaveCartItems(List<CartItem> cartItems)
        {
            var cartJson = JsonConvert.SerializeObject(cartItems);
            Session[CartSessionKey] = cartJson;
        }
        
        private int GetCartCount()
        {
            return GetCartItems().Sum(i => i.Quantity);
        }
        
        private decimal GetCartTotal()
        {
            var cartItems = GetCartItems();
            if (cartItems.Count == 0)
            {
                return 0;
            }
            
            var productIds = cartItems.Select(i => i.ProductId).ToList();
            var products = _db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToList();
            
            decimal total = 0;
            foreach (var item in cartItems)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    total += (product.DiscountPrice ?? product.Price) * item.Quantity;
                }
            }
            
            return total;
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
    
    // Вспомогательные классы для работы с корзиной
    public class CartItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
    
    // Класс для десериализации товаров из JavaScript корзины
    public class CartItemJS
    {
        public int id { get; set; }
        public string name { get; set; }
        public decimal price { get; set; }
        public int qty { get; set; }
        public string image { get; set; }
    }
    
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        public decimal SubTotal { get; set; }
        public int StockQuantity { get; set; }
    }
    
    public class CheckoutViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ShippingAddress { get; set; }
        public string BillingAddress { get; set; }
        public string PaymentMethod { get; set; }
        public string ShippingMethod { get; set; }
        public string Notes { get; set; }
    }
} 