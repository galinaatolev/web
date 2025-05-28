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
    public class ReviewController : Controller
    {
        // ✅ ПРАВИЛЬНО - используем Business Logic вместо прямого контекста
        private readonly IReviewBL _reviewBL;
        private readonly IProductBL _productBL;
        private readonly ICustomerBL _customerBL;

        public ReviewController()
        {
            _reviewBL = new ReviewBL();
            _productBL = new ProductBL();
            _customerBL = new CustomerBL();
        }

        // GET: /Review/Create/5 (где 5 - ID продукта)
        [Authorize]
        public ActionResult Create(int productId)
        {
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var product = _productBL.GetProductById(productId);
            if (product == null)
            {
                return HttpNotFound();
            }

            ViewBag.Product = product;
            return View();
        }

        // POST: /Review/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Review review)
        {
            if (!ModelState.IsValid)
            {
                // ✅ ПРАВИЛЬНО - используем Business Logic
                var product = _productBL.GetProductById(review.ProductId);
                ViewBag.Product = product;
                return View(review);
            }

            // Получаем текущего пользователя
            var username = User.Identity.Name;
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var customer = _customerBL.GetCustomerByUsername(username);
            
            if (customer == null)
            {
                return RedirectToAction("Login", "Profile", new { returnUrl = Url.Action("Create", "Review", new { productId = review.ProductId }) });
            }

            // Проверяем, может ли клиент оставить отзыв
            if (!_reviewBL.CanCustomerReview(customer.Id, review.ProductId))
            {
                TempData["ErrorMessage"] = "Вы уже оставляли отзыв на этот товар";
                return RedirectToAction("ProductDetails", "Home", new { id = review.ProductId });
            }

            // Заполняем информацию об отзыве
            review.CustomerId = customer.Id;

            // ✅ ПРАВИЛЬНО - используем Business Logic
            if (_reviewBL.CreateReview(review))
            {
                TempData["SuccessMessage"] = "Ваш отзыв отправлен на модерацию";
            }
            else
            {
                TempData["ErrorMessage"] = "Ошибка при создании отзыва";
            }

            return RedirectToAction("ProductDetails", "Home", new { id = review.ProductId });
        }

        // POST: /Review/Delete/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            // ✅ ПРАВИЛЬНО - используем Business Logic
            var review = _reviewBL.GetReviewById(id);

            if (review == null)
            {
                return HttpNotFound();
            }

            // Проверяем, является ли текущий пользователь автором отзыва или администратором
            var username = User.Identity.Name;
            var isAdmin = User.IsInRole("Admin");
            var isAuthor = _reviewBL.IsReviewOwnedByCustomer(id, _customerBL.GetCustomerByUsername(username)?.Id ?? 0);

            if (!isAdmin && !isAuthor)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
            }

            // Помечаем отзыв как удаленный вместо физического удаления
            if (_reviewBL.SoftDeleteReview(id))
            {
                TempData["SuccessMessage"] = "Отзыв удален";
            }
            else
            {
                TempData["ErrorMessage"] = "Ошибка при удалении отзыва";
            }

            return RedirectToAction("ProductDetails", "Home", new { id = review.ProductId });
        }

        // AJAX: Голосование за полезность отзыва
        [HttpPost]
        public ActionResult VoteHelpful(int reviewId, bool isHelpful)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, redirectToLogin = true, loginUrl = Url.Action("Login", "Profile", new { returnUrl = Request.UrlReferrer.ToString() }) });
            }

            // ✅ ПРАВИЛЬНО - используем Business Logic
            if (_reviewBL.VoteHelpful(reviewId, isHelpful))
            {
                var review = _reviewBL.GetReviewById(reviewId);
                return Json(new { 
                    success = true, 
                    helpfulVotes = review.HelpfulVotes, 
                    unhelpfulVotes = review.UnhelpfulVotes 
                });
            }
            else
            {
                return Json(new { success = false, message = "Review not found" });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                (_reviewBL as IDisposable)?.Dispose();
                (_productBL as IDisposable)?.Dispose();
                (_customerBL as IDisposable)?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
} 