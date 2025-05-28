using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BeautyMoldova.Application.Interfaces;
using BeautyMoldova.Application.BusinessLogic;
using BeautyMoldova.Domain.Models;
using Newtonsoft.Json;

namespace BeautyMoldova.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductBL _productBL;
        private readonly ICustomerBL _customerBL;
        private readonly IPurchaseBL _purchaseBL;
        private const string CartSessionKey = "CartItems";

        public CartController()
        {
            _productBL = new ProductBL();
            _customerBL = new CustomerBL();
            _purchaseBL = new PurchaseBL();
        }
        
        // Отображение страницы корзины
        public ActionResult Index()
        {
            var cartItems = GetCartItems();
            
            if (cartItems.Count == 0)
            {
                return View(new List<CartItemViewModel>());
            }
            
            var productIds = cartItems.Select(i => i.ProductId).ToList();
            var allProducts = _productBL.GetAllProducts();
            var products = allProducts.Where(p => productIds.Contains(p.Id)).ToList();
            
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
                        Price = product.DiscountPrice ?? product.Price,
                        Quantity = item.Quantity,
                        ImageUrl = product.MainImage,
                        SubTotal = (product.DiscountPrice ?? product.Price) * item.Quantity,
                        StockQuantity = product.StockQuantity
                    });
                }
            }
            
            return View(cartViewModels);
        }
        
        // Добавление товара в корзину
        [HttpPost]
        public ActionResult AddToCart(int productId, int quantity = 1)
        {
            var product = _productBL.GetProductById(productId);
            if (product == null || !product.IsAvailable)
            {
                return Json(new { success = false, message = "Товар не найден или недоступен" });
            }
            
            if (product.StockQuantity < quantity)
            {
                return Json(new { success = false, message = "Недостаточно товара на складе" });
            }
            
            var cartItems = GetCartItems();
            var existingItem = cartItems.FirstOrDefault(i => i.ProductId == productId);
            
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
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
            
            return Json(new { 
                success = true, 
                message = "Товар добавлен в корзину",
                cartCount = cartItems.Sum(i => i.Quantity)
            });
        }
        
        // Обновление количества товара в корзине
        [HttpPost]
        public ActionResult UpdateCartItem(int productId, int quantity)
        {
            var cartItems = GetCartItems();
            var item = cartItems.FirstOrDefault(i => i.ProductId == productId);
            
            if (item != null)
            {
                if (quantity <= 0)
                {
                    cartItems.Remove(item);
                }
                else
                {
                    var product = _productBL.GetProductById(productId);
                    if (product != null && product.StockQuantity >= quantity)
                    {
                        item.Quantity = quantity;
                    }
                    else
                    {
                        return Json(new { success = false, message = "Недостаточно товара на складе" });
                    }
                }
                
                SaveCartItems(cartItems);
                
                return Json(new { 
                    success = true,
                    cartCount = cartItems.Sum(i => i.Quantity)
                });
            }
            
            return Json(new { success = false, message = "Товар не найден в корзине" });
        }
        
        // Удаление товара из корзины
        [HttpPost]
        public ActionResult RemoveFromCart(int productId)
        {
            var cartItems = GetCartItems();
            var item = cartItems.FirstOrDefault(i => i.ProductId == productId);
            
            if (item != null)
            {
                cartItems.Remove(item);
                SaveCartItems(cartItems);
                
                return Json(new { 
                    success = true,
                    cartCount = cartItems.Sum(i => i.Quantity)
                });
            }
            
            return Json(new { success = false, message = "Товар не найден в корзине" });
        }
        
        // Очистка корзины
        [HttpPost]
        public ActionResult ClearCart()
        {
            Session[CartSessionKey] = null;
            return Json(new { success = true });
        }
        
        // Получение количества товаров в корзине
        public ActionResult GetCartCount()
        {
            var cartItems = GetCartItems();
            return Json(new { count = cartItems.Sum(i => i.Quantity) }, JsonRequestBehavior.AllowGet);
        }
        
        // Страница оформления заказа
        [Authorize]
        public ActionResult Checkout()
        {
            var cartItems = GetCartItems();
            if (cartItems.Count == 0)
            {
                return RedirectToAction("Index");
            }
            
            var productIds = cartItems.Select(i => i.ProductId).ToList();
            var allProducts = _productBL.GetAllProducts();
            var products = allProducts.Where(p => productIds.Contains(p.Id)).ToList();
            
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
                        Price = product.DiscountPrice ?? product.Price,
                        Quantity = item.Quantity,
                        ImageUrl = product.MainImage,
                        SubTotal = (product.DiscountPrice ?? product.Price) * item.Quantity,
                        StockQuantity = product.StockQuantity
                    });
                }
            }
            
            ViewBag.CartItems = cartViewModels;
            ViewBag.CartTotal = cartViewModels.Sum(i => i.SubTotal);
            
            return View(new CheckoutViewModel());
        }
        
        // Обработка заказа
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessCheckout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var cartItemsList = GetCartItems();
                var productsIds = cartItemsList.Select(i => i.ProductId).ToList();
                var allProducts = _productBL.GetAllProducts();
                var productsItems = allProducts.Where(p => productsIds.Contains(p.Id)).ToList();
                
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
                            Price = product.DiscountPrice ?? product.Price,
                            Quantity = item.Quantity,
                            ImageUrl = product.MainImage,
                            SubTotal = (product.DiscountPrice ?? product.Price) * item.Quantity,
                            StockQuantity = product.StockQuantity
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
            var customer = _customerBL.GetCustomerByUsername(username);
            
            if (customer == null)
            {
                return RedirectToAction("Login", "Profile", new { returnUrl = Url.Action("Checkout") });
            }
            
            var purchase = new Purchase
            {
                CustomerId = customer.Id,
                OrderNumber = "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                OrderDate = DateTime.Now,
                PurchaseDate = DateTime.Now,
                Status = "Pending",
                PaymentMethod = model.PaymentMethod,
                ShippingMethod = model.ShippingMethod,
                ShippingAddress = model.ShippingAddress,
                BillingAddress = model.BillingAddress,
                CustomerNotes = model.Notes,
                PurchaseItems = new List<PurchaseItem>()
            };
            
            decimal totalAmount = 0;
            var productIds = cartItems.Select(i => i.ProductId).ToList();
            var allProductsForOrder = _productBL.GetAllProducts();
            var productsForOrder = allProductsForOrder.Where(p => productIds.Contains(p.Id)).ToList();
            
            foreach (var cartItem in cartItems)
            {
                var product = productsForOrder.FirstOrDefault(p => p.Id == cartItem.ProductId);
                if (product != null && product.StockQuantity >= cartItem.Quantity)
                {
                    var price = product.DiscountPrice ?? product.Price;
                    var purchaseItem = new PurchaseItem
                    {
                        ProductId = product.Id,
                        Quantity = cartItem.Quantity,
                        UnitPrice = price,
                        TotalPrice = price * cartItem.Quantity
                    };
                    
                    purchase.PurchaseItems.Add(purchaseItem);
                    totalAmount += purchaseItem.TotalPrice;
                    
                    product.StockQuantity -= cartItem.Quantity;
                    _productBL.UpdateProduct(product);
                }
            }
            
            purchase.TotalAmount = totalAmount;
            
            if (_purchaseBL.CreatePurchase(purchase))
            {
                Session[CartSessionKey] = null;
                
                TempData["SuccessMessage"] = "Ваш заказ успешно оформлен!";
                return RedirectToAction("OrderConfirmation", new { id = purchase.Id });
            }
            else
            {
                TempData["ErrorMessage"] = "Произошла ошибка при оформлении заказа";
                return RedirectToAction("Checkout");
            }
        }
        
        // Подтверждение заказа
        [Authorize]
        public ActionResult OrderConfirmation(int id)
        {
            var purchase = _purchaseBL.GetPurchaseById(id);
            if (purchase == null)
            {
                return HttpNotFound();
            }
            
            var customer = _customerBL.GetCustomerByUsername(User.Identity.Name);
            if (customer == null || purchase.CustomerId != customer.Id)
            {
                return new HttpUnauthorizedResult();
            }
            
            return View(purchase);
        }
        
        private List<CartItem> GetCartItems()
        {
            var cartItemsJson = Session[CartSessionKey] as string;
            if (string.IsNullOrEmpty(cartItemsJson))
            {
                return new List<CartItem>();
            }
            
            try
            {
                return JsonConvert.DeserializeObject<List<CartItem>>(cartItemsJson) ?? new List<CartItem>();
            }
            catch
            {
                return new List<CartItem>();
            }
        }
        
        private void SaveCartItems(List<CartItem> cartItems)
        {
            Session[CartSessionKey] = JsonConvert.SerializeObject(cartItems);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                (_productBL as IDisposable)?.Dispose();
                (_customerBL as IDisposable)?.Dispose();
                (_purchaseBL as IDisposable)?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
    
    public class CartItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
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
        public string PaymentMethod { get; set; }
        public string ShippingMethod { get; set; }
        public string ShippingAddress { get; set; }
        public string BillingAddress { get; set; }
        public string Notes { get; set; }
    }
} 