using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BeautyMoldova.Application.Interfaces;
using BeautyMoldova.Application.BusinessLogic;
using BeautyMoldova.Domain.Models;
using System.Collections.Generic;

namespace BeautyMoldova.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        // ✅ ПРАВИЛЬНО - используем Business Logic вместо прямого контекста
        private readonly IProductBL _productBL;
        private readonly ICategoryBL _categoryBL;
        private readonly ICustomerBL _customerBL;
        private readonly IPurchaseBL _purchaseBL;

        public AdminController()
        {
            _productBL = new ProductBL();
            _categoryBL = new CategoryBL();
            _customerBL = new CustomerBL();
            _purchaseBL = new PurchaseBL();
        }

        // Дашборд администратора
        public ActionResult Index()
        {
            // ✅ ПРАВИЛЬНО - используем Business Logic
            ViewBag.OrdersCount = _purchaseBL.GetPurchasesCount();
            ViewBag.CustomersCount = _customerBL.GetCustomersCount();
            ViewBag.ProductsCount = _productBL.GetProductsCount();
            ViewBag.TotalRevenue = _purchaseBL.GetTotalRevenue();
            ViewBag.RecentOrders = _purchaseBL.GetRecentPurchases(5);
            ViewBag.NewCustomers = _customerBL.GetPagedCustomers(1, 5);
                
            return View();
        }
        
        #region Управление категориями
        
        // Список категорий
        public ActionResult Categories(string searchString = null, int? parentCategoryId = null)
        {
            // ✅ ПРАВИЛЬНО - используем Business Logic
            List<Category> categories;
            
            if (!string.IsNullOrEmpty(searchString))
            {
                categories = _categoryBL.SearchCategories(searchString);
            }
            else if (parentCategoryId.HasValue)
            {
                categories = _categoryBL.GetChildCategories(parentCategoryId.Value);
            }
            else
            {
                categories = _categoryBL.GetAllCategories();
            }
            
            // Получаем все категории верхнего уровня для фильтрации
            var parentCategories = _categoryBL.GetParentCategories();
            
            ViewBag.ParentCategories = parentCategories;
            ViewBag.SearchString = searchString;
            ViewBag.ParentCategoryId = parentCategoryId;
            
            return View(categories);
        }
        
        // Создание категории
        public ActionResult CreateCategory()
        {
            // Получаем список родительских категорий для выпадающего списка
            var parentCategories = _categoryBL.GetParentCategories();
            ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                // ✅ ПРАВИЛЬНО - используем Business Logic
                if (_categoryBL.CreateCategory(category))
                {
                    TempData["SuccessMessage"] = "Category has been created successfully";
                    return RedirectToAction("Categories");
                }
                else
                {
                    TempData["ErrorMessage"] = "Error creating category";
                }
            }

            var parentCategories = _categoryBL.GetParentCategories();
            ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");
            return View(category);
        }

        // Редактирование категории
        public ActionResult EditCategory(int id)
        {
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var category = _categoryBL.GetCategoryById(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            var parentCategories = _categoryBL.GetParentCategories();
            ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name", category.ParentCategoryId);
            
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                // ✅ ПРАВИЛЬНО - используем Business Logic
                if (_categoryBL.UpdateCategory(category))
                {
                    TempData["SuccessMessage"] = "Category has been updated successfully";
                    return RedirectToAction("Categories");
                }
                else
                {
                    TempData["ErrorMessage"] = "Error updating category";
                }
            }

            var parentCategories = _categoryBL.GetParentCategories();
            ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name", category.ParentCategoryId);
            return View(category);
        }

        // Удаление категории
        public ActionResult DeleteCategory(int id)
        {
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var category = _categoryBL.GetCategoryById(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            
            if (_categoryBL.HasProducts(id) || _categoryBL.HasChildCategories(id))
            {
                TempData["ErrorMessage"] = "Cannot delete category that has products or child categories. Please reassign them first.";
                return RedirectToAction("Categories");
            }
            
            return View(category);
        }

        [HttpPost, ActionName("DeleteCategory")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCategoryConfirmed(int id)
        {
            // ✅ ПРАВИЛЬНО - используем Business Logic
            if (_categoryBL.DeleteCategory(id))
            {
                TempData["SuccessMessage"] = "Category has been deleted successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Error deleting category or category has dependencies";
            }
            
            return RedirectToAction("Categories");
        }

        #endregion

        #region Управление продуктами

        // Список продуктов
        public ActionResult Products(string searchString = null, int? categoryId = null, int? manufacturerId = null)
        {
            // ✅ ПРАВИЛЬНО - используем Business Logic
            List<Product> products;
            
            if (!string.IsNullOrEmpty(searchString))
            {
                products = _productBL.SearchProducts(searchString);
            }
            else if (categoryId.HasValue)
            {
                products = _productBL.GetProductsByCategory(categoryId.Value);
            }
            else if (manufacturerId.HasValue)
            {
                products = _productBL.GetProductsByManufacturer(manufacturerId.Value);
            }
            else
            {
                products = _productBL.GetAllProducts();
            }
            
            // Загружаем список категорий для фильтров
            ViewBag.Categories = _categoryBL.GetAllCategories();
            
            // Передаем параметры фильтров в представление
            ViewBag.SearchString = searchString;
            ViewBag.CategoryId = categoryId;
            ViewBag.ManufacturerId = manufacturerId;
            
            return View(products);
        }
        
        // Создание продукта (GET)
        public ActionResult CreateProduct()
        {
            // ✅ ПРАВИЛЬНО - используем Business Logic
            ViewBag.Categories = new SelectList(_categoryBL.GetAllCategories().Where(c => c.IsActive), "Id", "Name");
            
            return View();
        }

        // Создание продукта (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateProduct(Product product, HttpPostedFileBase mainImage)
        {
            if (ModelState.IsValid)
            {
                // Обработка загруженного изображения
                if (mainImage != null && mainImage.ContentLength > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(mainImage.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/images"), fileName);
                    mainImage.SaveAs(path);
                    product.MainImage = fileName;
                }
                
                // ✅ ПРАВИЛЬНО - используем Business Logic
                if (_productBL.CreateProduct(product))
                {
                    TempData["SuccessMessage"] = "Product has been created successfully";
                    return RedirectToAction("Products");
                }
                else
                {
                    TempData["ErrorMessage"] = "Error creating product";
                }
            }

            ViewBag.Categories = new SelectList(_categoryBL.GetAllCategories().Where(c => c.IsActive), "Id", "Name");
            return View(product);
        }

        // Редактирование продукта
        public ActionResult EditProduct(int id)
        {
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var product = _productBL.GetProductById(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            ViewBag.Categories = new SelectList(_categoryBL.GetAllCategories().Where(c => c.IsActive), "Id", "Name", product.CategoryId);
            
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProduct(Product product, HttpPostedFileBase mainImage)
        {
            if (ModelState.IsValid)
            {
                // Обработка загруженного изображения
                if (mainImage != null && mainImage.ContentLength > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(mainImage.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/images"), fileName);
                    mainImage.SaveAs(path);
                    product.MainImage = fileName;
                }
                
                // ✅ ПРАВИЛЬНО - используем Business Logic
                if (_productBL.UpdateProduct(product))
                {
                    TempData["SuccessMessage"] = "Product has been updated successfully";
                    return RedirectToAction("Products");
                }
                else
                {
                    TempData["ErrorMessage"] = "Error updating product";
                }
            }

            ViewBag.Categories = new SelectList(_categoryBL.GetAllCategories().Where(c => c.IsActive), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // Удаление продукта
        public ActionResult DeleteProduct(int id)
        {
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var product = _productBL.GetProductById(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            
            return View(product);
        }

        [HttpPost, ActionName("DeleteProduct")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteProductConfirmed(int id)
        {
            // ✅ ПРАВИЛЬНО - используем Business Logic
            if (_productBL.DeleteProduct(id))
            {
                TempData["SuccessMessage"] = "Product has been deleted successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Error deleting product";
            }
            
            return RedirectToAction("Products");
        }

        #endregion

        #region Управление заказами

        // Список заказов
        public ActionResult Orders(string searchString = null, string status = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            // ✅ ПРАВИЛЬНО - используем Business Logic
            List<Purchase> orders;
            
            if (!string.IsNullOrEmpty(searchString))
            {
                orders = _purchaseBL.SearchPurchases(searchString);
            }
            else if (!string.IsNullOrEmpty(status))
            {
                orders = _purchaseBL.GetPurchasesByStatus(status);
            }
            else if (startDate.HasValue && endDate.HasValue)
            {
                orders = _purchaseBL.GetPurchasesByDateRange(startDate.Value, endDate.Value.AddDays(1));
            }
            else
            {
                orders = _purchaseBL.GetAllPurchases();
            }
            
            // Получаем список всех возможных статусов
            var statuses = new List<string> { "Processing", "Shipped", "Delivered", "Cancelled" };
            
            ViewBag.Statuses = statuses;
            ViewBag.CurrentStatus = status;
            ViewBag.SearchString = searchString;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            
            return View(orders);
        }
        
        // Детали заказа
        public ActionResult OrderDetails(int id)
        {
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var order = _purchaseBL.GetPurchaseById(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            
            return View(order);
        }
        
        // Обновление статуса заказа
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateOrderStatus(int id, string status, string trackingNumber = null)
        {
            // ✅ ПРАВИЛЬНО - используем Business Logic
            if (_purchaseBL.UpdatePurchaseStatus(id, status))
            {
                TempData["SuccessMessage"] = "Order status has been updated successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Error updating order status";
            }
            
            return RedirectToAction("OrderDetails", new { id = id });
        }
        
        // Отмена заказа
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelOrder(int id, string reason)
        {
            // ✅ ПРАВИЛЬНО - используем Business Logic
            if (_purchaseBL.CancelPurchase(id, reason))
            {
                TempData["SuccessMessage"] = "Order has been cancelled successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Error cancelling order";
            }
            
            return RedirectToAction("OrderDetails", new { id = id });
        }

        #endregion

        #region Управление клиентами

        // Список клиентов
        public ActionResult Customers(string searchString = null)
        {
            // ✅ ПРАВИЛЬНО - используем Business Logic
            List<Customer> customers;
            
            if (!string.IsNullOrEmpty(searchString))
            {
                customers = _customerBL.SearchCustomers(searchString);
            }
            else
            {
                customers = _customerBL.GetAllCustomers();
            }
            
            ViewBag.SearchString = searchString;
            return View(customers);
        }
        
        // Детали клиента
        public ActionResult CustomerDetails(int id)
        {
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var customer = _customerBL.GetCustomerById(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            
            return View(customer);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                (_productBL as IDisposable)?.Dispose();
                (_categoryBL as IDisposable)?.Dispose();
                (_customerBL as IDisposable)?.Dispose();
                (_purchaseBL as IDisposable)?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
} 