using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BeautyMoldova.Database;
using BeautyMoldova.Domain.Models;
using System.Collections.Generic;

namespace BeautyMoldova.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ShopDataContext _db;

        public AdminController()
        {
            _db = new ShopDataContext();
        }

        // Дашборд администратора
        public async Task<ActionResult> Index()
        {
            // Получаем количество заказов
            ViewBag.OrdersCount = await _db.Purchases.CountAsync();
            
            // Получаем количество пользователей
            ViewBag.CustomersCount = await _db.Customers.CountAsync();
            
            // Получаем количество продуктов
            ViewBag.ProductsCount = await _db.Products.CountAsync();
            
            // Получаем общий доход
            ViewBag.TotalRevenue = await _db.Purchases
                .Where(p => p.Status != "Cancelled")
                .SumAsync(p => p.TotalAmount);
            
            // Получаем последние заказы
            ViewBag.RecentOrders = await _db.Purchases
                .Include(p => p.Customer)
                .OrderByDescending(p => p.PurchaseDate)
                .Take(5)
                .ToListAsync();
            
            // Получаем новых пользователей
            ViewBag.NewCustomers = await _db.Customers
                .OrderByDescending(c => c.RegistrationDate)
                .Take(5)
                .ToListAsync();
                
            return View();
        }
        
        #region Управление категориями
        
        // Список категорий
        public async Task<ActionResult> Categories(string searchString = null, int? parentCategoryId = null)
        {
            var categoriesQuery = _db.Categories.AsQueryable();
            
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                categoriesQuery = categoriesQuery.Where(c => 
                    c.Name.ToLower().Contains(searchString) || 
                    c.Description.ToLower().Contains(searchString)
                );
            }
            
            if (parentCategoryId.HasValue)
            {
                categoriesQuery = categoriesQuery.Where(c => c.ParentCategoryId == parentCategoryId);
            }
            
            var categories = await categoriesQuery
                .Include(c => c.ParentCategory)
                .Include(c => c.Products)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
            
            // Получаем все категории верхнего уровня для фильтрации
            var parentCategories = await _db.Categories
                .Where(c => c.ParentCategoryId == null)
                .OrderBy(c => c.Name)
                .ToListAsync();
            
            ViewBag.ParentCategories = parentCategories;
            ViewBag.SearchString = searchString;
            ViewBag.ParentCategoryId = parentCategoryId;
            
            return View(categories);
        }
        
        // Создание категории
        public async Task<ActionResult> CreateCategory()
        {
            // Получаем все категории для выбора родительской категории
            var parentCategories = await _db.Categories
                .Where(c => c.ParentCategoryId == null)
                .OrderBy(c => c.Name)
                .ToListAsync();
            
            ViewBag.ParentCategories = parentCategories;
            
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateCategory(Category category, HttpPostedFileBase categoryImage)
        {
            if (ModelState.IsValid)
            {
                // Обработка загруженного изображения
                if (categoryImage != null && categoryImage.ContentLength > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(categoryImage.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/images"), fileName);
                    categoryImage.SaveAs(path);
                    category.ImageUrl = fileName;
                }
                
                // Генерация URL-слага, если не задан
                if (string.IsNullOrEmpty(category.Slug))
                {
                    category.Slug = GenerateSlug(category.Name);
                }
                
                _db.Categories.Add(category);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Category has been created successfully";
                return RedirectToAction("Categories");
            }
            
            // Если модель невалидна, получаем список категорий заново
            var parentCategories = await _db.Categories
                .Where(c => c.ParentCategoryId == null)
                .OrderBy(c => c.Name)
                .ToListAsync();
            
            ViewBag.ParentCategories = parentCategories;
            
            return View(category);
        }
        
        // Редактирование категории
        public async Task<ActionResult> EditCategory(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            
            // Получаем все категории для выбора родительской категории
            var parentCategories = await _db.Categories
                .Where(c => c.Id != id) // Исключаем текущую категорию
                .OrderBy(c => c.Name)
                .ToListAsync();
            
            ViewBag.ParentCategories = parentCategories;
            
            return View(category);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditCategory(Category category, HttpPostedFileBase categoryImage, bool RemoveImage = false)
        {
            if (ModelState.IsValid)
            {
                // Проверяем, существует ли категория с таким ID
                var existingCategory = await _db.Categories.FindAsync(category.Id);
                if (existingCategory == null)
                {
                    return HttpNotFound();
                }
                
                // Обработка загруженного изображения
                if (categoryImage != null && categoryImage.ContentLength > 0)
                {
                    // Удаляем старое изображение, если есть
                    if (!string.IsNullOrEmpty(existingCategory.ImageUrl))
                    {
                        string oldPath = Path.Combine(Server.MapPath("~/Content/images"), existingCategory.ImageUrl);
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }
                    
                    // Сохраняем новое изображение
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(categoryImage.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/images"), fileName);
                    categoryImage.SaveAs(path);
                    category.ImageUrl = fileName;
                }
                else if (RemoveImage)
                {
                    // Удаляем изображение, если выбрана опция удаления
                    if (!string.IsNullOrEmpty(existingCategory.ImageUrl))
                    {
                        string oldPath = Path.Combine(Server.MapPath("~/Content/images"), existingCategory.ImageUrl);
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }
                    category.ImageUrl = null;
                }
                else
                {
                    // Сохраняем текущее изображение
                    category.ImageUrl = existingCategory.ImageUrl;
                }
                
                // Генерация URL-слага, если не задан
                if (string.IsNullOrEmpty(category.Slug))
                {
                    category.Slug = GenerateSlug(category.Name);
                }
                
                _db.Entry(existingCategory).CurrentValues.SetValues(category);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Category has been updated successfully";
                return RedirectToAction("Categories");
            }
            
            // Если модель невалидна, получаем список категорий заново
            var parentCategories = await _db.Categories
                .Where(c => c.Id != category.Id) // Исключаем текущую категорию
                .OrderBy(c => c.Name)
                .ToListAsync();
            
            ViewBag.ParentCategories = parentCategories;
            
            return View(category);
        }
        
        // Удаление категории
        public async Task<ActionResult> DeleteCategory(int id)
        {
            var category = await _db.Categories
                .Include(c => c.Products)
                .Include(c => c.ChildCategories)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (category == null)
            {
                return HttpNotFound();
            }
            
            if (category.Products.Any() || category.ChildCategories.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete category that has products or child categories. Please reassign them first.";
                return RedirectToAction("Categories");
            }
            
            return View(category);
        }
        
        [HttpPost, ActionName("DeleteCategory")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteCategoryConfirmed(int id)
        {
            var category = await _db.Categories
                .Include(c => c.Products)
                .Include(c => c.ChildCategories)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (category == null)
            {
                return HttpNotFound();
            }
            
            if (category.Products.Any() || category.ChildCategories.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete category that has products or child categories. Please reassign them first.";
                return RedirectToAction("Categories");
            }
            
            // Удаляем изображение, если есть
            if (!string.IsNullOrEmpty(category.ImageUrl))
            {
                string imagePath = Path.Combine(Server.MapPath("~/Content/images"), category.ImageUrl);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
            
            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Category has been deleted successfully";
            return RedirectToAction("Categories");
        }
        
        #endregion
        
        #region Управление производителями
        
        // Список производителей
        public async Task<ActionResult> Manufacturers(string searchString = null, string country = null, bool? isLuxury = null)
        {
            var manufacturersQuery = _db.Manufacturers.AsQueryable();
            
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                manufacturersQuery = manufacturersQuery.Where(m => 
                    m.Name.ToLower().Contains(searchString) || 
                    m.Description.ToLower().Contains(searchString)
                );
            }
            
            if (!string.IsNullOrEmpty(country))
            {
                manufacturersQuery = manufacturersQuery.Where(m => m.Country == country);
            }
            
            if (isLuxury.HasValue)
            {
                manufacturersQuery = manufacturersQuery.Where(m => m.IsLuxury == isLuxury.Value);
            }
            
            var manufacturers = await manufacturersQuery
                .Include(m => m.Products)
                .OrderBy(m => m.SortOrder)
                .ToListAsync();
            
            // Получаем список уникальных стран для фильтрации
            var countries = await _db.Manufacturers
                .Where(m => !string.IsNullOrEmpty(m.Country))
                .Select(m => m.Country)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
            
            ViewBag.Countries = countries;
            ViewBag.SearchString = searchString;
            ViewBag.SelectedCountry = country;
            ViewBag.IsLuxury = isLuxury;
            
            return View(manufacturers);
        }
        
        // Создание производителя
        public ActionResult CreateManufacturer()
        {
            var manufacturer = new Manufacturer
            {
                IsActive = true,
                EstablishedYear = DateTime.Now.AddYears(-10) // Значение по умолчанию
            };
            
            return View(manufacturer);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateManufacturer(Manufacturer manufacturer, HttpPostedFileBase logoImage)
        {
            if (ModelState.IsValid)
            {
                // Обработка загруженного логотипа
                if (logoImage != null && logoImage.ContentLength > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(logoImage.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/images"), fileName);
                    logoImage.SaveAs(path);
                    manufacturer.Logo = fileName;
                }
                
                _db.Manufacturers.Add(manufacturer);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Manufacturer has been created successfully";
                return RedirectToAction("Manufacturers");
            }
            
            return View(manufacturer);
        }
        
        // Редактирование производителя
        public async Task<ActionResult> EditManufacturer(int id)
        {
            var manufacturer = await _db.Manufacturers
                .Include(m => m.Products)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (manufacturer == null)
            {
                return HttpNotFound();
            }
            
            return View(manufacturer);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditManufacturer(Manufacturer manufacturer, HttpPostedFileBase logoImage, bool RemoveLogo = false)
        {
            if (ModelState.IsValid)
            {
                // Проверяем, существует ли производитель с таким ID
                var existingManufacturer = await _db.Manufacturers.FindAsync(manufacturer.Id);
                if (existingManufacturer == null)
                {
                    return HttpNotFound();
                }
                
                // Обработка загруженного логотипа
                if (logoImage != null && logoImage.ContentLength > 0)
                {
                    // Удаляем старый логотип, если есть
                    if (!string.IsNullOrEmpty(existingManufacturer.Logo))
                    {
                        string oldPath = Path.Combine(Server.MapPath("~/Content/images"), existingManufacturer.Logo);
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }
                    
                    // Сохраняем новый логотип
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(logoImage.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/images"), fileName);
                    logoImage.SaveAs(path);
                    manufacturer.Logo = fileName;
                }
                else if (RemoveLogo)
                {
                    // Удаляем логотип, если выбрана опция удаления
                    if (!string.IsNullOrEmpty(existingManufacturer.Logo))
                    {
                        string oldPath = Path.Combine(Server.MapPath("~/Content/images"), existingManufacturer.Logo);
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }
                    manufacturer.Logo = null;
                }
                else
                {
                    // Сохраняем текущий логотип
                    manufacturer.Logo = existingManufacturer.Logo;
                }
                
                _db.Entry(existingManufacturer).CurrentValues.SetValues(manufacturer);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Manufacturer has been updated successfully";
                return RedirectToAction("Manufacturers");
            }
            
            return View(manufacturer);
        }
        
        // Удаление производителя
        public async Task<ActionResult> DeleteManufacturer(int id)
        {
            var manufacturer = await _db.Manufacturers
                .Include(m => m.Products)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (manufacturer == null)
            {
                return HttpNotFound();
            }
            
            if (manufacturer.Products.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete manufacturer that has products. Please reassign them first.";
                return RedirectToAction("Manufacturers");
            }
            
            return View(manufacturer);
        }
        
        [HttpPost, ActionName("DeleteManufacturer")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteManufacturerConfirmed(int id)
        {
            var manufacturer = await _db.Manufacturers
                .Include(m => m.Products)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (manufacturer == null)
            {
                return HttpNotFound();
            }
            
            if (manufacturer.Products.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete manufacturer that has products. Please reassign them first.";
                return RedirectToAction("Manufacturers");
            }
            
            // Удаляем логотип, если есть
            if (!string.IsNullOrEmpty(manufacturer.Logo))
            {
                string logoPath = Path.Combine(Server.MapPath("~/Content/images"), manufacturer.Logo);
                if (System.IO.File.Exists(logoPath))
                {
                    System.IO.File.Delete(logoPath);
                }
            }
            
            _db.Manufacturers.Remove(manufacturer);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Manufacturer has been deleted successfully";
            return RedirectToAction("Manufacturers");
        }
        
        #endregion
        
        #region Управление ролями
        
        // Список ролей
        public async Task<ActionResult> Roles()
        {
            var roles = await _db.Roles.OrderBy(r => r.Name).ToListAsync();
            return View(roles);
        }
        
        // Создание роли
        public ActionResult CreateRole()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateRole(Role role)
        {
            if (ModelState.IsValid)
            {
                role.CreatedDate = DateTime.Now;
                _db.Roles.Add(role);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Роль успешно создана";
                return RedirectToAction("Roles");
            }
            
            return View(role);
        }
        
        // Редактирование роли
        public async Task<ActionResult> EditRole(int id)
        {
            var role = await _db.Roles.FindAsync(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            
            return View(role);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditRole(Role role)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(role).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Роль успешно обновлена";
                return RedirectToAction("Roles");
            }
            
            return View(role);
        }
        
        // Удаление роли
        public async Task<ActionResult> DeleteRole(int id)
        {
            var role = await _db.Roles.FindAsync(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            
            return View(role);
        }
        
        [HttpPost, ActionName("DeleteRole")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteRoleConfirmed(int id)
        {
            var role = await _db.Roles.FindAsync(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            
            _db.Roles.Remove(role);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Роль успешно удалена";
            return RedirectToAction("Roles");
        }
        
        #endregion
        
        #region Управление пользователями
        
        // Список пользователей
        public async Task<ActionResult> Customers(string searchString = null)
        {
            var customersQuery = _db.Customers.AsQueryable();
            
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                customersQuery = customersQuery.Where(c => 
                    c.Username.ToLower().Contains(searchString) || 
                    c.Email.ToLower().Contains(searchString) || 
                    c.FirstName.ToLower().Contains(searchString) || 
                    c.LastName.ToLower().Contains(searchString)
                );
            }
            
            var customers = await customersQuery
                .OrderByDescending(c => c.RegistrationDate)
                .ToListAsync();
            
            return View(customers);
        }
        
        // Просмотр информации о пользователе
        public async Task<ActionResult> CustomerDetails(int id)
        {
            var customer = await _db.Customers
                .Include(c => c.Purchases)
                .Include(c => c.CustomerRoles.Select(cr => cr.Role))
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (customer == null)
            {
                return HttpNotFound();
            }
            
            return View(customer);
        }
        
        // Управление ролями пользователя
        public async Task<ActionResult> ManageCustomerRoles(int id)
        {
            var customer = await _db.Customers
                .Include(c => c.CustomerRoles.Select(cr => cr.Role))
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (customer == null)
            {
                return HttpNotFound();
            }
            
            var allRoles = await _db.Roles.Where(r => r.IsActive).ToListAsync();
            var customerRoleIds = customer.CustomerRoles.Select(cr => cr.RoleId).ToList();
            
            ViewBag.Customer = customer;
            ViewBag.AllRoles = allRoles;
            ViewBag.CustomerRoleIds = customerRoleIds;
            
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ManageCustomerRoles(int customerId, int[] selectedRoles)
        {
            var customer = await _db.Customers
                .Include(c => c.CustomerRoles)
                .FirstOrDefaultAsync(c => c.Id == customerId);
            
            if (customer == null)
            {
                return HttpNotFound();
            }
            
            // Удаляем существующие роли
            foreach (var customerRole in customer.CustomerRoles.ToList())
            {
                _db.CustomerRoles.Remove(customerRole);
            }
            
            // Добавляем выбранные роли
            if (selectedRoles != null)
            {
                foreach (var roleId in selectedRoles)
                {
                    customer.CustomerRoles.Add(new CustomerRole
                    {
                        CustomerId = customerId,
                        RoleId = roleId,
                        AssignedDate = DateTime.Now
                    });
                }
            }
            
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Роли пользователя успешно обновлены";
            return RedirectToAction("CustomerDetails", new { id = customerId });
        }
        
        #endregion
        
        #region Управление продуктами
        
        // Список продуктов
        public async Task<ActionResult> Products(string searchString = null, int? categoryId = null, int? manufacturerId = null)
        {
            var productsQuery = _db.Products.AsQueryable();
            
            // Применяем фильтры
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                productsQuery = productsQuery.Where(p => 
                    p.Name.ToLower().Contains(searchString) || 
                    p.Description.ToLower().Contains(searchString)
                );
            }
            
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }
            
            if (manufacturerId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.ManufacturerId == manufacturerId.Value);
            }
            
            // Загружаем список продуктов с включением связанных категорий и производителей
            var products = await productsQuery
                .Include(p => p.Category)
                .Include(p => p.Manufacturer)
                .OrderByDescending(p => p.AddedDate)
                .ToListAsync();
            
            // Загружаем список категорий и производителей для фильтров
            ViewBag.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Manufacturers = await _db.Manufacturers.OrderBy(m => m.Name).ToListAsync();
            
            // Передаем параметры фильтров в представление
            ViewBag.SearchString = searchString;
            ViewBag.CategoryId = categoryId;
            ViewBag.ManufacturerId = manufacturerId;
            
            return View(products);
        }
        
        // Создание продукта (GET)
        public async Task<ActionResult> CreateProduct()
        {
            // Загружаем список категорий и производителей для выпадающих списков
            ViewBag.Categories = new SelectList(await _db.Categories.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            ViewBag.Manufacturers = new SelectList(await _db.Manufacturers.Where(m => m.IsActive).OrderBy(m => m.Name).ToListAsync(), "Id", "Name");
            
            // Загружаем список тегов
            ViewBag.Tags = await _db.Tags.Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync();
            
            return View();
        }
        
        // Создание продукта (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateProduct(Product product, int[] selectedTags, HttpPostedFileBase mainImage)
        {
            if (ModelState.IsValid)
            {
                // Устанавливаем дату добавления
                product.AddedDate = DateTime.Now;
                
                // Обработка загруженного изображения
                if (mainImage != null && mainImage.ContentLength > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(mainImage.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/images"), fileName);
                    mainImage.SaveAs(path);
                    product.MainImage = fileName;
                }
                
                // Добавляем продукт в базу данных
                _db.Products.Add(product);
                await _db.SaveChangesAsync();
                
                // Добавляем выбранные теги
                if (selectedTags != null && selectedTags.Length > 0)
                {
                    foreach (var tagId in selectedTags)
                    {
                        _db.ProductTags.Add(new ProductTag
                        {
                            ProductId = product.Id,
                            TagId = tagId
                        });
                    }
                    await _db.SaveChangesAsync();
                }
                
                TempData["SuccessMessage"] = "Продукт успешно создан";
                return RedirectToAction("Products");
            }
            
            // Если модель невалидна, повторно загружаем списки для выпадающих списков
            ViewBag.Categories = new SelectList(await _db.Categories.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync(), "Id", "Name", product.CategoryId);
            ViewBag.Manufacturers = new SelectList(await _db.Manufacturers.Where(m => m.IsActive).OrderBy(m => m.Name).ToListAsync(), "Id", "Name", product.ManufacturerId);
            ViewBag.Tags = await _db.Tags.Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync();
            
            return View(product);
        }
        
        // Редактирование продукта (GET)
        public async Task<ActionResult> EditProduct(int id)
        {
            var product = await _db.Products
                .Include(p => p.ProductTags.Select(pt => pt.Tag))
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (product == null)
            {
                return HttpNotFound();
            }
            
            // Загружаем списки для выпадающих списков
            ViewBag.Categories = new SelectList(await _db.Categories.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync(), "Id", "Name", product.CategoryId);
            ViewBag.Manufacturers = new SelectList(await _db.Manufacturers.Where(m => m.IsActive).OrderBy(m => m.Name).ToListAsync(), "Id", "Name", product.ManufacturerId);
            
            // Загружаем список тегов и отмечаем выбранные
            ViewBag.Tags = await _db.Tags.Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync();
            ViewBag.SelectedTags = product.ProductTags.Select(pt => pt.TagId).ToList();
            
            return View(product);
        }
        
        // Редактирование продукта (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditProduct(Product product, int[] selectedTags, HttpPostedFileBase mainImage)
        {
            if (ModelState.IsValid)
            {
                // Получаем существующий продукт из базы данных
                var existingProduct = await _db.Products
                    .Include(p => p.ProductTags)
                    .FirstOrDefaultAsync(p => p.Id == product.Id);
                
                if (existingProduct == null)
                {
                    return HttpNotFound();
                }
                
                // Обновляем поля продукта
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.DiscountPrice = product.DiscountPrice;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.ManufacturerId = product.ManufacturerId;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.SKU = product.SKU;
                existingProduct.IsAvailable = product.IsAvailable;
                existingProduct.IsFeatured = product.IsFeatured;
                existingProduct.SkinType = product.SkinType;
                
                // Обработка загруженного изображения
                if (mainImage != null && mainImage.ContentLength > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(mainImage.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/images"), fileName);
                    mainImage.SaveAs(path);
                    
                    // Удаляем старое изображение, если оно есть
                    if (!string.IsNullOrEmpty(existingProduct.MainImage))
                    {
                        string oldImagePath = Path.Combine(Server.MapPath("~/Content/images"), existingProduct.MainImage);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    
                    existingProduct.MainImage = fileName;
                }
                
                // Обновляем теги
                // Удаляем существующие теги
                foreach (var productTag in existingProduct.ProductTags.ToList())
                {
                    _db.ProductTags.Remove(productTag);
                }
                
                // Добавляем выбранные теги
                if (selectedTags != null && selectedTags.Length > 0)
                {
                    foreach (var tagId in selectedTags)
                    {
                        _db.ProductTags.Add(new ProductTag
                        {
                            ProductId = product.Id,
                            TagId = tagId
                        });
                    }
                }
                
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Продукт успешно обновлен";
                return RedirectToAction("Products");
            }
            
            // Если модель невалидна, повторно загружаем списки для выпадающих списков
            ViewBag.Categories = new SelectList(await _db.Categories.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync(), "Id", "Name", product.CategoryId);
            ViewBag.Manufacturers = new SelectList(await _db.Manufacturers.Where(m => m.IsActive).OrderBy(m => m.Name).ToListAsync(), "Id", "Name", product.ManufacturerId);
            ViewBag.Tags = await _db.Tags.Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync();
            ViewBag.SelectedTags = selectedTags ?? new int[0];
            
            return View(product);
        }
        
        // Удаление продукта (GET)
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .Include(p => p.Manufacturer)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (product == null)
            {
                return HttpNotFound();
            }
            
            return View(product);
        }
        
        // Удаление продукта (POST)
        [HttpPost, ActionName("DeleteProduct")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteProductConfirmed(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            
            // Удаляем изображение, если оно есть
            if (!string.IsNullOrEmpty(product.MainImage))
            {
                string imagePath = Path.Combine(Server.MapPath("~/Content/images"), product.MainImage);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
            
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Продукт успешно удален";
            return RedirectToAction("Products");
        }
        
        #endregion
        
        #region Управление заказами
        
        // Список заказов
        public async Task<ActionResult> Orders(string searchString = null, string status = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var ordersQuery = _db.Purchases.AsQueryable();
            
            // Применение фильтров
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                ordersQuery = ordersQuery.Where(o => 
                    o.Id.ToString().Contains(searchString) || 
                    o.Customer.FirstName.ToLower().Contains(searchString) || 
                    o.Customer.LastName.ToLower().Contains(searchString) || 
                    o.Customer.Email.ToLower().Contains(searchString) || 
                    o.Customer.PhoneNumber.Contains(searchString)
                );
            }
            
            if (!string.IsNullOrEmpty(status))
            {
                ordersQuery = ordersQuery.Where(o => o.Status == status);
            }
            
            if (startDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.PurchaseDate >= startDate.Value);
            }
            
            if (endDate.HasValue)
            {
                // Добавляем один день к конечной дате, чтобы включить заказы в течение всего последнего дня
                ordersQuery = ordersQuery.Where(o => o.PurchaseDate <= endDate.Value.AddDays(1));
            }
            
            var orders = await ordersQuery
                .Include(o => o.Customer)
                .OrderByDescending(o => o.PurchaseDate)
                .ToListAsync();
            
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
        public async Task<ActionResult> OrderDetails(int id)
        {
            var order = await _db.Purchases
                .Include(o => o.Customer)
                .Include(o => o.PurchaseItems.Select(pi => pi.Product))
                .FirstOrDefaultAsync(o => o.Id == id);
            
            if (order == null)
            {
                return HttpNotFound();
            }
            
            return View(order);
        }
        
        // Обновление статуса заказа
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateOrderStatus(int id, string status, string trackingNumber = null)
        {
            var order = await _db.Purchases.FindAsync(id);
            
            if (order == null)
            {
                return HttpNotFound();
            }
            
            order.Status = status;
            
            if (!string.IsNullOrEmpty(trackingNumber))
            {
                order.TrackingNumber = trackingNumber;
            }
            
            // Обновляем даты в зависимости от статуса
            if (status == "Shipped" && !order.ShippingDate.HasValue)
            {
                order.ShippingDate = DateTime.Now;
            }
            else if (status == "Delivered" && !order.DeliveryDate.HasValue)
            {
                order.DeliveryDate = DateTime.Now;
            }
            
            await _db.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Order status has been updated successfully";
            return RedirectToAction("OrderDetails", new { id = id });
        }
        
        // Отмена заказа
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CancelOrder(int id, string cancellationReason)
        {
            var order = await _db.Purchases
                .Include(o => o.PurchaseItems.Select(pi => pi.Product))
                .FirstOrDefaultAsync(o => o.Id == id);
            
            if (order == null)
            {
                return HttpNotFound();
            }
            
            // Проверяем, можно ли отменить заказ
            if (order.Status == "Delivered")
            {
                TempData["ErrorMessage"] = "Cannot cancel an order that has already been delivered";
                return RedirectToAction("OrderDetails", new { id = id });
            }
            
            // Отменяем заказ
            order.Status = "Cancelled";
            order.AdminNotes = (order.AdminNotes ?? "") + Environment.NewLine + 
                $"Cancelled on {DateTime.Now.ToString("g")}. Reason: {cancellationReason}";
            
            // Возвращаем товары на склад
            foreach (var item in order.PurchaseItems)
            {
                item.Product.StockQuantity += item.Quantity;
            }
            
            await _db.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Order has been cancelled successfully";
            return RedirectToAction("OrderDetails", new { id = id });
        }
        
        // Добавление примечания к заказу
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddOrderNote(int id, string note)
        {
            var order = await _db.Purchases.FindAsync(id);
            
            if (order == null)
            {
                return HttpNotFound();
            }
            
            if (!string.IsNullOrEmpty(note))
            {
                order.AdminNotes = (order.AdminNotes ?? "") + Environment.NewLine + 
                    $"[{DateTime.Now.ToString("g")}] {note}";
                
                await _db.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Note has been added successfully";
            }
            
            return RedirectToAction("OrderDetails", new { id = id });
        }
        
        // Экспорт заказов в CSV
        public async Task<ActionResult> ExportOrdersToCSV(string searchString = null, string status = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var ordersQuery = _db.Purchases.AsQueryable();
            
            // Применение тех же фильтров, что и в методе Orders
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                ordersQuery = ordersQuery.Where(o => 
                    o.Id.ToString().Contains(searchString) || 
                    o.Customer.FirstName.ToLower().Contains(searchString) || 
                    o.Customer.LastName.ToLower().Contains(searchString) || 
                    o.Customer.Email.ToLower().Contains(searchString) || 
                    o.Customer.PhoneNumber.Contains(searchString)
                );
            }
            
            if (!string.IsNullOrEmpty(status))
            {
                ordersQuery = ordersQuery.Where(o => o.Status == status);
            }
            
            if (startDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.PurchaseDate >= startDate.Value);
            }
            
            if (endDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.PurchaseDate <= endDate.Value.AddDays(1));
            }
            
            var orders = await ordersQuery
                .Include(o => o.Customer)
                .OrderByDescending(o => o.PurchaseDate)
                .ToListAsync();
            
            // Создаем CSV-файл
            var csv = new System.Text.StringBuilder();
            
            // Заголовки колонок
            csv.AppendLine("Order ID,Customer Name,Customer Email,Date,Total Amount,Status,Payment Method,Shipping Method");
            
            // Данные заказов
            foreach (var order in orders)
            {
                csv.AppendLine($"{order.Id}," +
                               $"\"{order.Customer.FirstName} {order.Customer.LastName}\"," +
                               $"\"{order.Customer.Email}\"," +
                               $"{order.PurchaseDate.ToString("yyyy-MM-dd HH:mm")}," +
                               $"{order.TotalAmount}," +
                               $"{order.Status}," +
                               $"{order.PaymentMethod}," +
                               $"{order.ShippingMethod}");
            }
            
            // Возвращаем CSV-файл
            var filename = $"orders_export_{DateTime.Now.ToString("yyyy-MM-dd_HHmmss")}.csv";
            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", filename);
        }
        
        #endregion
        
        #region Вспомогательные методы
        
        // Генерация URL-слага из строки
        private string GenerateSlug(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            
            // Приводим к нижнему регистру
            input = input.ToLower();
            
            // Заменяем специальные символы на дефисы
            input = System.Text.RegularExpressions.Regex.Replace(input, @"[^a-z0-9\s-]", "");
            
            // Заменяем пробелы на дефисы
            input = System.Text.RegularExpressions.Regex.Replace(input, @"\s+", "-");
            
            // Заменяем множественные дефисы на один
            input = System.Text.RegularExpressions.Regex.Replace(input, @"-+", "-");
            
            // Обрезаем начальные и конечные дефисы
            input = input.Trim('-');
            
            return input;
        }
        
        #endregion
        
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