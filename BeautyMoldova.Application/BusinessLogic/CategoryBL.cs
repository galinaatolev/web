using System.Collections.Generic;
using System.Linq;
using BeautyMoldova.Application.Interfaces;
using BeautyMoldova.Database;
using BeautyMoldova.Domain.Models;

namespace BeautyMoldova.Application.BusinessLogic
{
    /// <summary>
    /// Business Logic класс для управления категориями
    /// Наследуется от BaseApi и делегирует все операции к базовым CRUD методам
    /// </summary>
    public class CategoryBL : BaseApi, ICategoryBL
    {
        public CategoryBL() : base() { }
        public CategoryBL(ShopDataContext context) : base(context) { }

        #region Основные операции с категориями

        /// <summary>
        /// Получить категорию по ID
        /// ✅ ПРАВИЛЬНО - использует базовый метод
        /// </summary>
        public Category GetCategoryById(int id)
        {
            return GetById<Category>(id);
        }

        /// <summary>
        /// Получить все категории
        /// ✅ ПРАВИЛЬНО - использует базовый метод
        /// </summary>
        public List<Category> GetAllCategories()
        {
            return GetAll<Category>();
        }

        /// <summary>
        /// Получить родительские категории (категории верхнего уровня)
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<Category> GetParentCategories()
        {
            var allCategories = GetAll<Category>();
            return allCategories.Where(c => c.ParentCategoryId == null && c.IsActive)
                               .OrderBy(c => c.DisplayOrder)
                               .ToList();
        }

        /// <summary>
        /// Получить дочерние категории
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<Category> GetChildCategories(int parentId)
        {
            var allCategories = GetAll<Category>();
            return allCategories.Where(c => c.ParentCategoryId == parentId && c.IsActive)
                               .OrderBy(c => c.DisplayOrder)
                               .ToList();
        }

        /// <summary>
        /// Поиск категорий по тексту
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<Category> SearchCategories(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return new List<Category>();

            var allCategories = GetAll<Category>();
            searchTerm = searchTerm.ToLower();
            
            return allCategories.Where(c => 
                c.IsActive && (
                    c.Name.ToLower().Contains(searchTerm) ||
                    c.Description.ToLower().Contains(searchTerm)
                )).ToList();
        }

        #endregion

        #region CRUD операции

        /// <summary>
        /// Создать новую категорию
        /// ✅ ПРАВИЛЬНО - использует базовый метод Create
        /// </summary>
        public bool CreateCategory(Category category)
        {
            if (category == null) return false;
            return Create<Category>(category);
        }

        /// <summary>
        /// Обновить категорию
        /// ✅ ПРАВИЛЬНО - использует базовый метод Update
        /// </summary>
        public bool UpdateCategory(Category category)
        {
            if (category == null) return false;
            return Update<Category>(category);
        }

        /// <summary>
        /// Удалить категорию
        /// ✅ ПРАВИЛЬНО - использует базовый метод Delete
        /// </summary>
        public bool DeleteCategory(int id)
        {
            // Проверяем, есть ли продукты в категории
            if (HasProducts(id) || HasChildCategories(id))
            {
                return false; // Нельзя удалить категорию с продуктами или дочерними категориями
            }
            
            return Delete<Category>(id);
        }

        #endregion

        #region Дополнительные операции

        /// <summary>
        /// Получить количество категорий
        /// ✅ ПРАВИЛЬНО - использует базовый метод Count
        /// </summary>
        public int GetCategoriesCount()
        {
            return Count<Category>();
        }

        /// <summary>
        /// Получить категории с пагинацией
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetPaged
        /// </summary>
        public List<Category> GetPagedCategories(int page, int pageSize)
        {
            return GetPaged<Category>(page, pageSize);
        }

        /// <summary>
        /// Проверить, есть ли продукты в категории
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll для продуктов
        /// </summary>
        public bool HasProducts(int categoryId)
        {
            var allProducts = GetAll<Product>();
            return allProducts.Any(p => p.CategoryId == categoryId);
        }

        /// <summary>
        /// Проверить, есть ли дочерние категории
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll для категорий
        /// </summary>
        public bool HasChildCategories(int categoryId)
        {
            var allCategories = GetAll<Category>();
            return allCategories.Any(c => c.ParentCategoryId == categoryId);
        }

        #endregion

        #region Операции с продуктами категории

        /// <summary>
        /// Получить продукты категории
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll для продуктов, затем фильтрует
        /// </summary>
        public List<Product> GetCategoryProducts(int categoryId)
        {
            var allProducts = GetAll<Product>();
            return allProducts.Where(p => p.CategoryId == categoryId && p.IsAvailable).ToList();
        }

        /// <summary>
        /// Получить количество продуктов в категории
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll для продуктов, затем считает
        /// </summary>
        public int GetCategoryProductsCount(int categoryId)
        {
            var allProducts = GetAll<Product>();
            return allProducts.Count(p => p.CategoryId == categoryId && p.IsAvailable);
        }

        #endregion
    }
} 