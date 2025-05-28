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
    public class CatalogController : Controller
    {
        // ✅ ПРАВИЛЬНО - используем Business Logic вместо прямого контекста
        private readonly IProductBL _productBL;
        private readonly ICategoryBL _categoryBL;

        public CatalogController()
        {
            _productBL = new ProductBL();
            _categoryBL = new CategoryBL();
        }

        // Страница каталога продуктов с фильтрацией и пагинацией
        public ActionResult Index(int? categoryId = null, int? manufacturerId = null, string sortOrder = null, decimal? minPrice = null, decimal? maxPrice = null, string skinType = null, int page = 1)
        {
            ViewBag.Title = "Каталог продуктов";
            
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var products = _productBL.GetAllProducts().Where(p => p.IsAvailable);
            
            // Применяем фильтры
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
                var category = _categoryBL.GetCategoryById(categoryId.Value);
                ViewBag.CategoryName = category?.Name;
            }
            
            if (manufacturerId.HasValue)
            {
                products = products.Where(p => p.ManufacturerId == manufacturerId.Value);
            }
            
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.DiscountPrice.HasValue 
                    ? p.DiscountPrice >= minPrice.Value 
                    : p.Price >= minPrice.Value);
            }
            
            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.DiscountPrice.HasValue 
                    ? p.DiscountPrice <= maxPrice.Value 
                    : p.Price <= maxPrice.Value);
            }
            
            if (!string.IsNullOrEmpty(skinType))
            {
                products = products.Where(p => p.SkinType == skinType);
            }
            
            // Сортировка
            switch (sortOrder)
            {
                case "name_desc":
                    products = products.OrderByDescending(p => p.Name);
                    break;
                case "price":
                    products = products.OrderBy(p => p.DiscountPrice ?? p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.DiscountPrice ?? p.Price);
                    break;
                case "newest":
                    products = products.OrderByDescending(p => p.AddedDate);
                    break;
                default:
                    products = products.OrderBy(p => p.Name);
                    break;
            }
            
            // Пагинация
            const int pageSize = 12;
            var totalProducts = products.Count();
            var pagedProducts = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            
            // Данные для фильтров
            ViewBag.Categories = _categoryBL.GetAllCategories().Where(c => c.IsActive);
            ViewBag.SkinTypes = new[] { "Сухая", "Жирная", "Комбинированная", "Нормальная", "Чувствительная" };
            
            // Параметры пагинации
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            ViewBag.TotalProducts = totalProducts;
            
            // Текущие фильтры
            ViewBag.CurrentCategoryId = categoryId;
            ViewBag.CurrentManufacturerId = manufacturerId;
            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;
            ViewBag.CurrentSkinType = skinType;
            
            return View(pagedProducts);
        }
        
        // Детальная страница продукта
        public ActionResult ProductDetails(int id)
        {
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var product = _productBL.GetProductById(id);
            if (product == null || !product.IsAvailable)
            {
                return HttpNotFound();
            }
            
            // Увеличиваем счетчик просмотров
            _productBL.IncrementViewCount(id);
            
            // Получаем похожие товары из той же категории
            var relatedProducts = _productBL.GetProductsByCategory(product.CategoryId)
                                           .Where(p => p.Id != id && p.IsAvailable)
                                           .Take(4)
                                           .ToList();
            
            ViewBag.RelatedProducts = relatedProducts;
            
            return View(product);
        }
        
        // Поиск продуктов
        public ActionResult Search(string q, int page = 1)
        {
            ViewBag.Title = "Результаты поиска";
            ViewBag.SearchQuery = q;
            
            if (string.IsNullOrEmpty(q))
            {
                return View("Index", new List<Product>());
            }
            
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var products = _productBL.SearchProducts(q);
            
            // Пагинация
            const int pageSize = 12;
            var totalProducts = products.Count;
            var pagedProducts = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            ViewBag.TotalProducts = totalProducts;
            
            return View("Index", pagedProducts);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                (_productBL as IDisposable)?.Dispose();
                (_categoryBL as IDisposable)?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
} 