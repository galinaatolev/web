using System.Collections.Generic;
using BeautyMoldova.Domain.Models;

namespace BeautyMoldova.Application.Interfaces
{
    public interface ICategoryBL
    {
        // Основные операции с категориями
        Category GetCategoryById(int id);
        List<Category> GetAllCategories();
        List<Category> GetParentCategories();
        List<Category> GetChildCategories(int parentId);
        List<Category> SearchCategories(string searchTerm);
        
        // CRUD операции
        bool CreateCategory(Category category);
        bool UpdateCategory(Category category);
        bool DeleteCategory(int id);
        
        // Дополнительные операции
        int GetCategoriesCount();
        List<Category> GetPagedCategories(int page, int pageSize);
        bool HasProducts(int categoryId);
        bool HasChildCategories(int categoryId);
        
        // Операции с продуктами категории
        List<Product> GetCategoryProducts(int categoryId);
        int GetCategoryProductsCount(int categoryId);
    }
} 