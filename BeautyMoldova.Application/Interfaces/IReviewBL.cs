using System.Collections.Generic;
using BeautyMoldova.Domain.Models;

namespace BeautyMoldova.Application.Interfaces
{
    public interface IReviewBL
    {
        // Основные операции с отзывами
        Review GetReviewById(int id);
        List<Review> GetAllReviews();
        List<Review> GetProductReviews(int productId);
        List<Review> GetCustomerReviews(int customerId);
        List<Review> GetApprovedReviews(int productId);
        
        // CRUD операции
        bool CreateReview(Review review);
        bool UpdateReview(Review review);
        bool DeleteReview(int id);
        bool SoftDeleteReview(int id); // Помечает как удаленный
        
        // Дополнительные операции
        int GetReviewsCount();
        int GetProductReviewsCount(int productId);
        double GetProductAverageRating(int productId);
        List<Review> GetPagedReviews(int page, int pageSize);
        
        // Операции с голосованием
        bool VoteHelpful(int reviewId, bool isHelpful);
        
        // Проверки
        bool HasCustomerPurchasedProduct(int customerId, int productId);
        bool IsReviewOwnedByCustomer(int reviewId, int customerId);
        bool CanCustomerReview(int customerId, int productId);
        
        // Модерация
        bool ApproveReview(int reviewId);
        bool RejectReview(int reviewId);
        List<Review> GetPendingReviews();
    }
} 