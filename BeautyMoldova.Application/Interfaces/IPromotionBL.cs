using System;
using System.Collections.Generic;
using BeautyMoldova.Domain.Models;

namespace BeautyMoldova.Application.Interfaces
{
    public interface IPromotionBL
    {
        // Основные операции с акциями
        Promotion GetPromotionById(int id);
        List<Promotion> GetAllPromotions();
        List<Promotion> GetActivePromotions();
        List<Promotion> SearchPromotions(string searchTerm);
        
        // CRUD операции
        bool CreatePromotion(Promotion promotion);
        bool UpdatePromotion(Promotion promotion);
        bool DeletePromotion(int id);
        
        // Дополнительные операции
        int GetPromotionsCount();
        List<Promotion> GetPagedPromotions(int page, int pageSize);
        
        // Операции с товарами со скидкой
        List<Product> GetDiscountedProducts(int? categoryId = null);
        List<Product> GetPagedDiscountedProducts(int page, int pageSize, int? categoryId = null);
        int GetDiscountedProductsCount(int? categoryId = null);
        
        // Проверка валидности акции
        bool IsPromotionValid(int promotionId);
        bool IsPromotionActive(int promotionId);
    }
} 