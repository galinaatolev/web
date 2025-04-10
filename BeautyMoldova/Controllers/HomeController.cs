using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using BeautyMoldova.Database;
using BeautyMoldova.Domain.Models;

namespace CosmeticShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ShopDataContext _dataStore = new ShopDataContext();
        
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "About our company";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact us for more information";
            return View();
        }

        public ActionResult Products()
        {
            var products = _dataStore.Products
                .Include(p => p.Manufacturer)
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .ToList();
            return View(products);
        }
        
        public ActionResult ProductDetails(int id)
        {
            var product = _dataStore.Products
                .Include(p => p.Manufacturer)
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .Include(p => p.AdditionalImages)
                .FirstOrDefault(p => p.Id == id);
                
            if (product == null)
            {
                return HttpNotFound();
            }
            
            return View(product);
        }
        
        [HttpPost]
        [Authorize]
        public ActionResult AddToCart(int productId, int quantity)
        {
            var product = _dataStore.Products.Find(productId);
            if (product == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Produsul nu există");
            }
            
            // Здесь обычно была бы корзина в сессии или базе данных
            // Возвращаем заглушку для примера
            return Json(new { success = true, message = "Produsul a fost adăugat în coș" });
        }
        
        [HttpPost]
        [Authorize]
        public ActionResult CreatePurchase(Purchase purchase)
        {
            if (purchase == null || !purchase.PurchaseItems.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Date comandă invalide");
            }

            var customer = _dataStore.Customers.FirstOrDefault(c => c.Username == User.Identity.Name);
            if (customer == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Utilizator neautorizat");
            }
            
            purchase.CustomerId = customer.Id;
            purchase.OrderDate = DateTime.Now;
            purchase.Status = "Pending";
            purchase.OrderNumber = "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            
            _dataStore.Purchases.Add(purchase);
            _dataStore.SaveChanges();

            return Json(new { success = true, orderId = purchase.Id });
        }
    }
}