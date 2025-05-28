using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeautyMoldova.Application.Interfaces;
using BeautyMoldova.Application.BusinessLogic;
using BeautyMoldova.Domain.Models;

namespace BeautyMoldova.Controllers
{
    public class PromotionController : Controller
    {
        private readonly IPromotionBL _promotionBL;
        private readonly ICategoryBL _categoryBL;

        public PromotionController()
        {
            _promotionBL = new PromotionBL();
            _categoryBL = new CategoryBL();
        }
        
        // Список всех акций
        public ActionResult Index()
        {
            var activePromotions = _promotionBL.GetActivePromotions();
            
            return View(activePromotions);
        }
        
        // Детальная страница акции
        public ActionResult Details(int id)
        {
            var promotion = _promotionBL.GetPromotionById(id);
            
            if (promotion == null || !promotion.IsActive)
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
        public ActionResult Discounts(int? categoryId = null, int page = 1)
        {
            ViewBag.Title = "Товары со скидкой";
            
            const int pageSize = 12;
            var totalItems = _promotionBL.GetDiscountedProductsCount(categoryId);
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;
            
            var products = _promotionBL.GetPagedDiscountedProducts(page, pageSize, categoryId);
            
            // Подготовка данных для фильтров
            var categories = _categoryBL.GetAllCategories().Where(c => c.IsActive).OrderBy(c => c.DisplayOrder).ToList();
            
            if (categoryId.HasValue)
            {
                var category = _categoryBL.GetCategoryById(categoryId.Value);
                ViewBag.CategoryName = category?.Name;
            }
            
            ViewBag.Categories = categories;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.CategoryId = categoryId;
            
            return View(products);
        }
        
        // Поиск по акциям
        [HttpPost]
        public ActionResult Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("Index");
            }
            
            query = query.ToLower();
            
            var promotions = _promotionBL.SearchPromotions(query);
            
            ViewBag.SearchQuery = query;
            return View("Index", promotions);
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                (_promotionBL as IDisposable)?.Dispose();
                (_categoryBL as IDisposable)?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
} 