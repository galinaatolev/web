using System;
using System.Collections.Generic;
using System.Linq;
using BeautyMoldova.Application.Interfaces;
using BeautyMoldova.Database;
using BeautyMoldova.Domain.Models;

namespace BeautyMoldova.Application.BusinessLogic
{
    /// <summary>
    /// Business Logic класс для управления акциями
    /// Наследуется от BaseApi и делегирует все операции к базовым CRUD методам
    /// </summary>
    public class PromotionBL : BaseApi, IPromotionBL
    {
        public PromotionBL() : base() { }
        public PromotionBL(ShopDataContext context) : base(context) { }

        #region Основные операции с акциями

        /// <summary>
        /// Получить акцию по ID
        /// ✅ ПРАВИЛЬНО - использует базовый метод
        /// </summary>
        public Promotion GetPromotionById(int id)
        {
            return GetById<Promotion>(id);
        }

        /// <summary>
        /// Получить все акции
        /// ✅ ПРАВИЛЬНО - использует базовый метод
        /// </summary>
        public List<Promotion> GetAllPromotions()
        {
            return GetAll<Promotion>();
        }

        /// <summary>
        /// Получить активные акции
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<Promotion> GetActivePromotions()
        {
            var allPromotions = GetAll<Promotion>();
            return allPromotions.Where(p => p.IsActive && p.EndDate >= DateTime.Now)
                               .OrderBy(p => p.EndDate)
                               .ToList();
        }

        /// <summary>
        /// Поиск акций по тексту
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<Promotion> SearchPromotions(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return new List<Promotion>();

            var allPromotions = GetAll<Promotion>();
            searchTerm = searchTerm.ToLower();
            
            return allPromotions.Where(p => 
                p.IsActive && p.EndDate >= DateTime.Now && (
                    p.Title.ToLower().Contains(searchTerm) ||
                    p.Description.ToLower().Contains(searchTerm) ||
                    (p.PromoCode != null && p.PromoCode.ToLower().Contains(searchTerm))
                )).OrderBy(p => p.EndDate).ToList();
        }

        #endregion

        #region CRUD операции

        /// <summary>
        /// Создать новую акцию
        /// ✅ ПРАВИЛЬНО - использует базовый метод Create
        /// </summary>
        public bool CreatePromotion(Promotion promotion)
        {
            if (promotion == null) return false;
            
            // Устанавливаем значения по умолчанию
            promotion.UsageCount = 0;
            promotion.IsActive = true;
            
            return Create<Promotion>(promotion);
        }

        /// <summary>
        /// Обновить акцию
        /// ✅ ПРАВИЛЬНО - использует базовый метод Update
        /// </summary>
        public bool UpdatePromotion(Promotion promotion)
        {
            if (promotion == null) return false;
            
            return Update<Promotion>(promotion);
        }

        /// <summary>
        /// Удалить акцию
        /// ✅ ПРАВИЛЬНО - использует базовый метод Delete
        /// </summary>
        public bool DeletePromotion(int id)
        {
            return Delete<Promotion>(id);
        }

        #endregion

        #region Дополнительные операции

        /// <summary>
        /// Получить количество акций
        /// ✅ ПРАВИЛЬНО - использует базовый метод Count
        /// </summary>
        public int GetPromotionsCount()
        {
            return Count<Promotion>();
        }

        /// <summary>
        /// Получить акции с пагинацией
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetPaged
        /// </summary>
        public List<Promotion> GetPagedPromotions(int page, int pageSize)
        {
            return GetPaged<Promotion>(page, pageSize);
        }

        /// <summary>
        /// Получить товары со скидкой
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<Product> GetDiscountedProducts(int? categoryId = null)
        {
            var allProducts = GetAll<Product>();
            var discountedProducts = allProducts.Where(p => p.IsAvailable && p.DiscountPrice.HasValue);
            
            if (categoryId.HasValue)
            {
                discountedProducts = discountedProducts.Where(p => p.CategoryId == categoryId.Value);
            }
            
            return discountedProducts.OrderByDescending(p => 
                (p.Price - p.DiscountPrice.Value) / p.Price * 100).ToList();
        }

        /// <summary>
        /// Получить товары со скидкой с пагинацией
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует и применяет пагинацию
        /// </summary>
        public List<Product> GetPagedDiscountedProducts(int page, int pageSize, int? categoryId = null)
        {
            var discountedProducts = GetDiscountedProducts(categoryId);
            return discountedProducts.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }

        /// <summary>
        /// Получить количество товаров со скидкой
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public int GetDiscountedProductsCount(int? categoryId = null)
        {
            return GetDiscountedProducts(categoryId).Count;
        }

        /// <summary>
        /// Проверить валидность акции
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetById
        /// </summary>
        public bool IsPromotionValid(int promotionId)
        {
            var promotion = GetById<Promotion>(promotionId);
            return promotion != null && promotion.IsActive && promotion.EndDate >= DateTime.Now;
        }

        /// <summary>
        /// Проверить активность акции
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetById
        /// </summary>
        public bool IsPromotionActive(int promotionId)
        {
            var promotion = GetById<Promotion>(promotionId);
            return promotion != null && promotion.IsActive;
        }

        #endregion
    }
} 