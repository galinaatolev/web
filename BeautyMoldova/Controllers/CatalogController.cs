using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeautyMoldova.Database;
using BeautyMoldova.Domain.Models;

namespace BeautyMoldova.Controllers
{
    public class CatalogController : Controller
    {
        private readonly ShopDataContext _db;

        public CatalogController()
        {
            _db = new ShopDataContext();
        }

        // Страница каталога продуктов с фильтрацией и пагинацией
        public async Task<ActionResult> Index(int? categoryId = null, int? manufacturerId = null, string sortOrder = null, decimal? minPrice = null, decimal? maxPrice = null, string skinType = null, int page = 1)
        {
            ViewBag.Title = "Каталог продуктов";
            
            var productsQuery = _db.Products.Where(p => p.IsAvailable).AsQueryable();
            
            // Применяем фильтры
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
                var category = await _db.Categories.FindAsync(categoryId.Value);
                ViewBag.CategoryName = category?.Name;
            }
            
            if (manufacturerId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.ManufacturerId == manufacturerId.Value);
                var manufacturer = await _db.Manufacturers.FindAsync(manufacturerId.Value);
                ViewBag.ManufacturerName = manufacturer?.Name;
            }
            
            if (minPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.DiscountPrice.HasValue 
                    ? p.DiscountPrice >= minPrice.Value 
                    : p.Price >= minPrice.Value);
            }
            
            if (maxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.DiscountPrice.HasValue 
                    ? p.DiscountPrice <= maxPrice.Value 
                    : p.Price <= maxPrice.Value);
            }
            
            if (!string.IsNullOrEmpty(skinType))
            {
                productsQuery = productsQuery.Where(p => p.SkinType == skinType);
            }
            
            // Сортировка
            switch (sortOrder)
            {
                case "price_asc":
                    productsQuery = productsQuery.OrderBy(p => p.DiscountPrice ?? p.Price);
                    break;
                case "price_desc":
                    productsQuery = productsQuery.OrderByDescending(p => p.DiscountPrice ?? p.Price);
                    break;
                case "name_asc":
                    productsQuery = productsQuery.OrderBy(p => p.Name);
                    break;
                case "name_desc":
                    productsQuery = productsQuery.OrderByDescending(p => p.Name);
                    break;
                case "new":
                    productsQuery = productsQuery.OrderByDescending(p => p.AddedDate);
                    break;
                case "popular":
                    productsQuery = productsQuery.OrderByDescending(p => p.ViewCount);
                    break;
                default:
                    productsQuery = productsQuery.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.AddedDate);
                    break;
            }
            
            // Пагинация
            int pageSize = 12;
            int totalItems = await productsQuery.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;
            
            var products = await productsQuery
                .Include(p => p.Manufacturer)
                .Include(p => p.Category)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            // Подготовка данных для фильтров
            ViewBag.Categories = await _db.Categories.Where(c => c.IsActive).OrderBy(c => c.DisplayOrder).ToListAsync();
            ViewBag.Manufacturers = await _db.Manufacturers.Where(m => m.IsActive).OrderBy(m => m.Name).ToListAsync();
            ViewBag.SkinTypes = await _db.Products.Where(p => !string.IsNullOrEmpty(p.SkinType))
                .Select(p => p.SkinType).Distinct().ToListAsync();
            
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.SortOrder = sortOrder;
            ViewBag.CategoryId = categoryId;
            ViewBag.ManufacturerId = manufacturerId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SkinType = skinType;
            
            return View(products);
        }
        
        // Страница детальной информации о продукте
        public async Task<ActionResult> ProductDetails(int id)
        {
            var product = await _db.Products
                .Include(p => p.Manufacturer)
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .Include(p => p.AdditionalImages)
                .Include(p => p.ProductTags.Select(pt => pt.Tag))
                .FirstOrDefaultAsync(p => p.Id == id && p.IsAvailable);
            
            if (product == null)
            {
                return HttpNotFound();
            }
            
            // Увеличиваем счетчик просмотров
            product.ViewCount++;
            await _db.SaveChangesAsync();
            
            // Получаем похожие продукты
            var relatedProducts = await _db.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.IsAvailable)
                .OrderByDescending(p => p.ViewCount)
                .Take(4)
                .ToListAsync();
            
            ViewBag.RelatedProducts = relatedProducts;
            
            // Проверяем, есть ли товар в вишлисте текущего пользователя
            if (User.Identity.IsAuthenticated)
            {
                var customerUsername = User.Identity.Name;
                var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Username == customerUsername);
                if (customer != null)
                {
                    ViewBag.IsInWishlist = await _db.WishlistItems.AnyAsync(w => w.CustomerId == customer.Id && w.ProductId == id);
                }
            }
            
            return View(product);
        }
        
        // Страница категорий
        public async Task<ActionResult> Categories()
        {
            var categories = await _db.Categories
                .Where(c => c.IsActive && c.ParentCategoryId == null)
                .Include(c => c.ChildCategories)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
            
            return View(categories);
        }
        
        // Страница товаров категории
        public async Task<ActionResult> Category(int id, int page = 1)
        {
            var category = await _db.Categories
                .Include(c => c.ChildCategories)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
            
            if (category == null)
            {
                return HttpNotFound();
            }
            
            var categoryIds = new[] { id };
            if (category.ChildCategories != null && category.ChildCategories.Any())
            {
                categoryIds = categoryIds.Concat(category.ChildCategories.Select(c => c.Id)).ToArray();
            }
            
            return RedirectToAction("Index", new { categoryId = id, page });
        }
        
        // Страница производителей
        public async Task<ActionResult> Manufacturers()
        {
            var manufacturers = await _db.Manufacturers
                .Where(m => m.IsActive)
                .OrderByDescending(m => m.IsLuxury)
                .ThenBy(m => m.SortOrder)
                .ThenBy(m => m.Name)
                .ToListAsync();
            
            return View(manufacturers);
        }
        
        // Страница товаров производителя
        public async Task<ActionResult> Manufacturer(int id, int page = 1)
        {
            var manufacturer = await _db.Manufacturers
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);
            
            if (manufacturer == null)
            {
                return HttpNotFound();
            }
            
            return RedirectToAction("Index", new { manufacturerId = id, page });
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