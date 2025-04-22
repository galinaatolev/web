using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeautyMoldova.Database;
using BeautyMoldova.Domain.Models;

namespace BeautyMoldova.Controllers
{
    public class PromotionController : Controller
    {
        private readonly ShopDataContext _db;

        public PromotionController()
        {
            _db = new ShopDataContext();
        }
        
        // Список всех акций
        public async Task<ActionResult> Index()
        {
            var activePromotions = await _db.Promotions
                .Where(p => p.IsActive && p.EndDate >= DateTime.Now)
                .OrderBy(p => p.EndDate)
                .ToListAsync();
            
            return View(activePromotions);
        }
        
        // Детальная страница акции
        public async Task<ActionResult> Details(int id)
        {
            var promotion = await _db.Promotions
                .Include(p => p.PromotionProducts.Select(pp => pp.Product.Manufacturer))
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
            
            if (promotion == null)
            {
                return HttpNotFound();
            }
            
            // Если акция истекла, перенаправляем на страницу списка
            if (promotion.EndDate < DateTime.Now)
            {
                return RedirectToAction("Index");
            }
            
            return View(promotion);
        }
        
        // Список товаров со скидкой
        public async Task<ActionResult> Discounts(int? categoryId = null, int page = 1)
        {
            ViewBag.Title = "Товары со скидкой";
            
            var productsQuery = _db.Products
                .Where(p => p.IsAvailable && p.DiscountPrice.HasValue)
                .AsQueryable();
            
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
                var category = await _db.Categories.FindAsync(categoryId.Value);
                ViewBag.CategoryName = category?.Name;
            }
            
            // Сортируем по размеру скидки (по убыванию процента скидки)
            productsQuery = productsQuery.OrderByDescending(p => 
                (p.Price - p.DiscountPrice.Value) / p.Price * 100);
            
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
            
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.CategoryId = categoryId;
            
            return View(products);
        }
        
        // Поиск по акциям
        [HttpPost]
        public async Task<ActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("Index");
            }
            
            query = query.ToLower();
            
            var promotions = await _db.Promotions
                .Where(p => p.IsActive && p.EndDate >= DateTime.Now &&
                           (p.Title.ToLower().Contains(query) || 
                            p.Description.ToLower().Contains(query) ||
                            p.PromoCode.ToLower().Contains(query)))
                .OrderBy(p => p.EndDate)
                .ToListAsync();
            
            ViewBag.SearchQuery = query;
            return View("Index", promotions);
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