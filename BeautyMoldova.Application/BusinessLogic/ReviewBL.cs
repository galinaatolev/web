using System;
using System.Collections.Generic;
using System.Linq;
using BeautyMoldova.Application.Interfaces;
using BeautyMoldova.Database;
using BeautyMoldova.Domain.Models;

namespace BeautyMoldova.Application.BusinessLogic
{
    /// <summary>
    /// Business Logic класс для управления отзывами
    /// Наследуется от BaseApi и делегирует все операции к базовым CRUD методам
    /// </summary>
    public class ReviewBL : BaseApi, IReviewBL
    {
        public ReviewBL() : base() { }
        public ReviewBL(ShopDataContext context) : base(context) { }

        #region Основные операции с отзывами

        /// <summary>
        /// Получить отзыв по ID
        /// ✅ ПРАВИЛЬНО - использует базовый метод
        /// </summary>
        public Review GetReviewById(int id)
        {
            return GetById<Review>(id);
        }

        /// <summary>
        /// Получить все отзывы
        /// ✅ ПРАВИЛЬНО - использует базовый метод
        /// </summary>
        public List<Review> GetAllReviews()
        {
            return GetAll<Review>();
        }

        /// <summary>
        /// Получить отзывы продукта
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<Review> GetProductReviews(int productId)
        {
            var allReviews = GetAll<Review>();
            return allReviews.Where(r => r.ProductId == productId && !r.IsDeleted)
                           .OrderByDescending(r => r.CreatedDate)
                           .ToList();
        }

        /// <summary>
        /// Получить отзывы клиента
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<Review> GetCustomerReviews(int customerId)
        {
            var allReviews = GetAll<Review>();
            return allReviews.Where(r => r.CustomerId == customerId && !r.IsDeleted)
                           .OrderByDescending(r => r.CreatedDate)
                           .ToList();
        }

        /// <summary>
        /// Получить одобренные отзывы продукта
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<Review> GetApprovedReviews(int productId)
        {
            var allReviews = GetAll<Review>();
            return allReviews.Where(r => r.ProductId == productId && r.IsApproved && !r.IsDeleted)
                           .OrderByDescending(r => r.CreatedDate)
                           .ToList();
        }

        #endregion

        #region CRUD операции

        /// <summary>
        /// Создать новый отзыв
        /// ✅ ПРАВИЛЬНО - использует базовый метод Create
        /// </summary>
        public bool CreateReview(Review review)
        {
            if (review == null) return false;
            
            review.CreatedAt = DateTime.Now;
            review.CreatedDate = DateTime.Now;
            review.IsApproved = false; // Требует одобрения администратором
            review.IsDeleted = false;
            review.HelpfulVotes = 0;
            review.UnhelpfulVotes = 0;
            
            // Проверяем, покупал ли пользователь этот товар
            review.IsVerifiedPurchase = HasCustomerPurchasedProduct(review.CustomerId, review.ProductId);
            
            return Create<Review>(review);
        }

        /// <summary>
        /// Обновить отзыв
        /// ✅ ПРАВИЛЬНО - использует базовый метод Update
        /// </summary>
        public bool UpdateReview(Review review)
        {
            if (review == null) return false;
            
            return Update<Review>(review);
        }

        /// <summary>
        /// Удалить отзыв
        /// ✅ ПРАВИЛЬНО - использует базовый метод Delete
        /// </summary>
        public bool DeleteReview(int id)
        {
            return Delete<Review>(id);
        }

        /// <summary>
        /// Мягкое удаление отзыва (помечает как удаленный)
        /// ✅ ПРАВИЛЬНО - использует базовые методы GetById и Update
        /// </summary>
        public bool SoftDeleteReview(int id)
        {
            var review = GetById<Review>(id);
            if (review != null)
            {
                review.IsDeleted = true;
                return Update<Review>(review);
            }
            return false;
        }

        #endregion

        #region Дополнительные операции

        /// <summary>
        /// Получить количество отзывов
        /// ✅ ПРАВИЛЬНО - использует базовый метод Count
        /// </summary>
        public int GetReviewsCount()
        {
            return Count<Review>();
        }

        /// <summary>
        /// Получить количество отзывов продукта
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public int GetProductReviewsCount(int productId)
        {
            return GetProductReviews(productId).Count;
        }

        /// <summary>
        /// Получить средний рейтинг продукта
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public double GetProductAverageRating(int productId)
        {
            var approvedReviews = GetApprovedReviews(productId);
            if (approvedReviews.Count == 0) return 0;
            
            return approvedReviews.Average(r => r.Rating);
        }

        /// <summary>
        /// Получить отзывы с пагинацией
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetPaged
        /// </summary>
        public List<Review> GetPagedReviews(int page, int pageSize)
        {
            return GetPaged<Review>(page, pageSize);
        }

        /// <summary>
        /// Голосование за полезность отзыва
        /// ✅ ПРАВИЛЬНО - использует базовые методы GetById и Update
        /// </summary>
        public bool VoteHelpful(int reviewId, bool isHelpful)
        {
            var review = GetById<Review>(reviewId);
            if (review != null)
            {
                if (isHelpful)
                {
                    review.HelpfulVotes = (review.HelpfulVotes ?? 0) + 1;
                }
                else
                {
                    review.UnhelpfulVotes = (review.UnhelpfulVotes ?? 0) + 1;
                }
                
                return Update<Review>(review);
            }
            return false;
        }

        #endregion

        #region Проверки

        /// <summary>
        /// Проверить, покупал ли клиент продукт
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public bool HasCustomerPurchasedProduct(int customerId, int productId)
        {
            var allPurchaseItems = GetAll<PurchaseItem>();
            return allPurchaseItems.Any(pi => pi.ProductId == productId && 
                                            pi.Purchase.CustomerId == customerId);
        }

        /// <summary>
        /// Проверить принадлежность отзыва клиенту
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetById
        /// </summary>
        public bool IsReviewOwnedByCustomer(int reviewId, int customerId)
        {
            var review = GetById<Review>(reviewId);
            return review != null && review.CustomerId == customerId;
        }

        /// <summary>
        /// Проверить, может ли клиент оставить отзыв
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public bool CanCustomerReview(int customerId, int productId)
        {
            // Проверяем, что клиент еще не оставлял отзыв на этот продукт
            var allReviews = GetAll<Review>();
            var existingReview = allReviews.FirstOrDefault(r => r.CustomerId == customerId && 
                                                              r.ProductId == productId && 
                                                              !r.IsDeleted);
            return existingReview == null;
        }

        #endregion

        #region Модерация

        /// <summary>
        /// Одобрить отзыв
        /// ✅ ПРАВИЛЬНО - использует базовые методы GetById и Update
        /// </summary>
        public bool ApproveReview(int reviewId)
        {
            var review = GetById<Review>(reviewId);
            if (review != null)
            {
                review.IsApproved = true;
                return Update<Review>(review);
            }
            return false;
        }

        /// <summary>
        /// Отклонить отзыв
        /// ✅ ПРАВИЛЬНО - использует базовые методы GetById и Update
        /// </summary>
        public bool RejectReview(int reviewId)
        {
            var review = GetById<Review>(reviewId);
            if (review != null)
            {
                review.IsApproved = false;
                return Update<Review>(review);
            }
            return false;
        }

        /// <summary>
        /// Получить отзывы, ожидающие модерации
        /// ✅ ПРАВИЛЬНО - использует базовый метод GetAll, затем фильтрует
        /// </summary>
        public List<Review> GetPendingReviews()
        {
            var allReviews = GetAll<Review>();
            return allReviews.Where(r => !r.IsApproved && !r.IsDeleted)
                           .OrderBy(r => r.CreatedDate)
                           .ToList();
        }

        #endregion
    }
} 