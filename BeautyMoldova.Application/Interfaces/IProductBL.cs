using System.Collections.Generic;
using BeautyMoldova.Domain.Models;

namespace BeautyMoldova.Application.Interfaces
{
    public interface IProductBL
    {
        // Основные операции с продуктами
        Product GetProductById(int id);
        List<Product> GetAllProducts();
        List<Product> GetProductsByCategory(int categoryId);
        List<Product> GetProductsByManufacturer(int manufacturerId);
        List<Product> GetFeaturedProducts();
        List<Product> SearchProducts(string searchTerm);
        
        // CRUD операции
        bool CreateProduct(Product product);
        bool UpdateProduct(Product product);
        bool DeleteProduct(int id);
        
        // Дополнительные операции
        int GetProductsCount();
        List<Product> GetPagedProducts(int page, int pageSize);
        bool IsProductAvailable(int productId);
        void IncrementViewCount(int productId);
        
        // Операции с изображениями
        bool AddProductImage(ProductImage image);
        bool RemoveProductImage(int imageId);
        List<ProductImage> GetProductImages(int productId);
        
        // Операции с тегами
        bool AddProductTag(int productId, int tagId);
        bool RemoveProductTag(int productId, int tagId);
        List<Tag> GetProductTags(int productId);
    }
} 