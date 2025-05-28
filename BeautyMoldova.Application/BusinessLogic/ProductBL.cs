using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using BeautyMoldova.Application.Interfaces;
using BeautyMoldova.Database;
using BeautyMoldova.Domain.Models;

namespace BeautyMoldova.Application.BusinessLogic
{
    /// <summary>
    /// Business Logic класс для управления продуктами
    /// Наследуется от BaseApi и делегирует все операции к базовым CRUD методам
    /// </summary>
    public class ProductBL : BaseApi, IProductBL
    {
        public ProductBL() : base() { }
        public ProductBL(ShopDataContext context) : base(context) { }

        #region Основные операции с продуктами

        /// <summary>
        /// Получить продукт по ID
        /// ✅ ПРАВИЛЬНО - использует базовый метод
        /// </summary>
        public Product GetProductById(int id)
        {
            return GetById<Product>(id);
        }

        /// <summary>
        /// Получить все продукты
        /// ✅ ПРАВИЛЬНО - использует базовый метод
        /// </summary>
        public List<Product> GetAllProducts()
        {
            return GetAll<Product>();
        }

        /// <summary>
        /// Получить продукты по категории
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<Product> GetProductsByCategory(int categoryId)
        {
            var allProducts = GetAll<Product>();
            return allProducts.Where(p => p.CategoryId == categoryId && p.IsAvailable).ToList();
        }

        /// <summary>
        /// Получить продукты по производителю
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<Product> GetProductsByManufacturer(int manufacturerId)
        {
            var allProducts = GetAll<Product>();
            return allProducts.Where(p => p.ManufacturerId == manufacturerId && p.IsAvailable).ToList();
        }

        /// <summary>
        /// Получить рекомендуемые продукты
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<Product> GetFeaturedProducts()
        {
            var allProducts = GetAll<Product>();
            return allProducts.Where(p => p.IsFeatured && p.IsAvailable).ToList();
        }

        /// <summary>
        /// Поиск продуктов по тексту
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<Product> SearchProducts(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return new List<Product>();

            var allProducts = GetAll<Product>();
            searchTerm = searchTerm.ToLower();
            
            return allProducts.Where(p => 
                p.IsAvailable && (
                    p.Name.ToLower().Contains(searchTerm) ||
                    p.Description.ToLower().Contains(searchTerm) ||
                    p.SKU.ToLower().Contains(searchTerm)
                )).ToList();
        }

        #endregion

        #region CRUD операции

        /// <summary>
        /// Создать новый продукт
        /// ✅ ПРАВИЛЬНО - использует базовый метод Create
        /// </summary>
        public bool CreateProduct(Product product)
        {
            if (product == null) return false;
            
            product.AddedDate = DateTime.Now;
            product.ViewCount = 0;
            
            return Create<Product>(product);
        }

        /// <summary>
        /// Обновить продукт
        /// ✅ ПРАВИЛЬНО - использует базовый метод Update
        /// </summary>
        public bool UpdateProduct(Product product)
        {
            if (product == null) return false;
            
            product.LastModifiedDate = DateTime.Now;
            
            return Update<Product>(product);
        }

        /// <summary>
        /// Удалить продукт
        /// ✅ ПРАВИЛЬНО - использует базовый метод Delete
        /// </summary>
        public bool DeleteProduct(int id)
        {
            return Delete<Product>(id);
        }

        #endregion

        #region Дополнительные операции

        /// <summary>
        /// Получить количество продуктов
        /// ✅ ПРАВИЛЬНО - использует базовый метод Count
        /// </summary>
        public int GetProductsCount()
        {
            return Count<Product>();
        }

        /// <summary>
        /// Получить продукты с пагинацией
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetPaged
        /// </summary>
        public List<Product> GetPagedProducts(int page, int pageSize)
        {
            return GetPaged<Product>(page, pageSize);
        }

        /// <summary>
        /// Проверить доступность продукта
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetById
        /// </summary>
        public bool IsProductAvailable(int productId)
        {
            var product = GetById<Product>(productId);
            return product != null && product.IsAvailable && product.StockQuantity > 0;
        }

        /// <summary>
        /// Увеличить счетчик просмотров
        /// ✅ ПРАВИЛЬНО - использует базовые методы GetById и Update
        /// </summary>
        public void IncrementViewCount(int productId)
        {
            var product = GetById<Product>(productId);
            if (product != null)
            {
                product.ViewCount++;
                Update<Product>(product);
            }
        }

        #endregion

        #region Операции с изображениями

        /// <summary>
        /// Добавить изображение продукта
        /// ✅ ПРАВИЛЬНО - использует базовый метод Create
        /// </summary>
        public bool AddProductImage(ProductImage image)
        {
            if (image == null) return false;
            return Create<ProductImage>(image);
        }

        /// <summary>
        /// Удалить изображение продукта
        /// ✅ ПРАВИЛЬНО - использует базовый метод Delete
        /// </summary>
        public bool RemoveProductImage(int imageId)
        {
            return Delete<ProductImage>(imageId);
        }

        /// <summary>
        /// Получить изображения продукта
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<ProductImage> GetProductImages(int productId)
        {
            var allImages = GetAll<ProductImage>();
            return allImages.Where(img => img.ProductId == productId && img.IsActive)
                           .OrderBy(img => img.DisplayOrder)
                           .ToList();
        }

        #endregion

        #region Операции с тегами

        /// <summary>
        /// Добавить тег к продукту
        /// ✅ ПРАВИЛЬНО - использует базовый метод Create
        /// </summary>
        public bool AddProductTag(int productId, int tagId)
        {
            var productTag = new ProductTag
            {
                ProductId = productId,
                TagId = tagId
            };
            return Create<ProductTag>(productTag);
        }

        /// <summary>
        /// Удалить тег у продукта
        /// ✅ ПРАВИЛЬНО - использует базовые методы GetAll и Delete
        /// </summary>
        public bool RemoveProductTag(int productId, int tagId)
        {
            var allProductTags = GetAll<ProductTag>();
            var productTag = allProductTags.FirstOrDefault(pt => pt.ProductId == productId && pt.TagId == tagId);
            
            if (productTag != null)
            {
                return Delete<ProductTag>(productTag);
            }
            return false;
        }

        /// <summary>
        /// Получить теги продукта
        /// ✅ ПРАВИЛЬНО - использует базовые методы GetAll для получения связей и тегов
        /// </summary>
        public List<Tag> GetProductTags(int productId)
        {
            var allProductTags = GetAll<ProductTag>();
            var allTags = GetAll<Tag>();
            
            var productTagIds = allProductTags.Where(pt => pt.ProductId == productId)
                                             .Select(pt => pt.TagId)
                                             .ToList();
            
            return allTags.Where(t => productTagIds.Contains(t.Id) && t.IsActive).ToList();
        }

        #endregion
    }
} 