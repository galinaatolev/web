using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BeautyMoldova.Application.Interfaces;
using BeautyMoldova.Application.BusinessLogic;
using BeautyMoldova.Domain.Models;

namespace CosmeticShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductBL _productBL;
        private readonly ICustomerBL _customerBL;
        private readonly IPurchaseBL _purchaseBL;

        public HomeController()
        {
            _productBL = new ProductBL();
            _customerBL = new CustomerBL();
            _purchaseBL = new PurchaseBL();
        }
        
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
            var products = _productBL.GetAllProducts();
            return View(products);
        }
        
        public ActionResult ProductDetails(int id)
        {
            var product = _productBL.GetProductById(id);
                
            if (product == null)
            {
                return HttpNotFound();
            }
            
            _productBL.IncrementViewCount(id);
            
            return View(product);
        }
        
        [HttpPost]
        [Authorize]
        public ActionResult AddToCart(int productId, int quantity)
        {
            var product = _productBL.GetProductById(productId);
            if (product == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Produsul nu există");
            }
            
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

            var customer = _customerBL.GetCustomerByUsername(User.Identity.Name);
            if (customer == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Utilizator neautorizat");
            }
            
            purchase.CustomerId = customer.Id;
            purchase.OrderDate = DateTime.Now;
            purchase.Status = "Pending";
            purchase.OrderNumber = "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            
            if (_purchaseBL.CreatePurchase(purchase))
            {
                return Json(new { success = true, orderId = purchase.Id });
            }
            
            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Eroare la crearea comenzii");
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
}