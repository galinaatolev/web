using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeautyMoldova.Database;
using BeautyMoldova.Domain.Models;

namespace BeautyMoldova.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ShopDataContext _db;

        public ReviewController()
        {
            _db = new ShopDataContext();
        }

        // GET: /Review/Create/5 (где 5 - ID продукта)
        [Authorize]
        public async Task<ActionResult> Create(int productId)
        {
            var product = await _db.Products.FindAsync(productId);
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
        public async Task<ActionResult> Create(Review review)
        {
            if (!ModelState.IsValid)
            {
                var product = await _db.Products.FindAsync(review.ProductId);
                ViewBag.Product = product;
                return View(review);
            }

            // Получаем текущего пользователя
            var username = User.Identity.Name;
            var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Username == username);
            
            if (customer == null)
            {
                return RedirectToAction("Login", "Profile", new { returnUrl = Url.Action("Create", "Review", new { productId = review.ProductId }) });
            }

            // Заполняем информацию об отзыве
            review.CustomerId = customer.Id;
            review.CreatedAt = DateTime.Now;
            review.CreatedDate = DateTime.Now;
            review.IsApproved = false; // Отзыв требует одобрения администратором
            review.IsDeleted = false;
            review.HelpfulVotes = 0;
            review.UnhelpfulVotes = 0;

            // Проверяем, покупал ли пользователь этот товар
            var hasPurchased = await _db.PurchaseItems
                .Where(pi => pi.Product.Id == review.ProductId)
                .Where(pi => pi.Purchase.Customer.Id == customer.Id)
                .AnyAsync();

            review.IsVerifiedPurchase = hasPurchased;

            _db.Reviews.Add(review);
            await _db.SaveChangesAsync();

            return RedirectToAction("ProductDetails", "Home", new { id = review.ProductId });
        }

        // POST: /Review/Delete/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            var review = await _db.Reviews
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                return HttpNotFound();
            }

            // Проверяем, является ли текущий пользователь автором отзыва или администратором
            var username = User.Identity.Name;
            var isAdmin = User.IsInRole("Admin");
            var isAuthor = review.Customer.Username == username;

            if (!isAdmin && !isAuthor)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
            }

            // Помечаем отзыв как удаленный вместо физического удаления
            review.IsDeleted = true;
            await _db.SaveChangesAsync();

            return RedirectToAction("ProductDetails", "Home", new { id = review.ProductId });
        }

        // AJAX: Голосование за полезность отзыва
        [HttpPost]
        public async Task<ActionResult> VoteHelpful(int reviewId, bool isHelpful)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, redirectToLogin = true, loginUrl = Url.Action("Login", "Profile", new { returnUrl = Request.UrlReferrer.ToString() }) });
            }

            var review = await _db.Reviews.FindAsync(reviewId);
            if (review == null)
            {
                return Json(new { success = false, message = "Review not found" });
            }

            if (isHelpful)
            {
                review.HelpfulVotes = (review.HelpfulVotes ?? 0) + 1;
            }
            else
            {
                review.UnhelpfulVotes = (review.UnhelpfulVotes ?? 0) + 1;
            }

            await _db.SaveChangesAsync();

            return Json(new { 
                success = true, 
                helpfulVotes = review.HelpfulVotes, 
                unhelpfulVotes = review.UnhelpfulVotes 
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
} 