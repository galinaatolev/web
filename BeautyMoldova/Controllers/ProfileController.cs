using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using BeautyMoldova.Application;
using BeautyMoldova.Application.Interfaces;
using BeautyMoldova.Application.BusinessLogic;
using BeautyMoldova.Domain.Models;
using System.Linq;

namespace CosmeticShop.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IdentityManager _identityManager;
        private readonly ICustomerBL _customerBL;

        public ProfileController()
        {
            _identityManager = new IdentityManager();
            _customerBL = new CustomerBL();
        }

        // GET: Страница входа в систему
        public ActionResult Enter(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Обработка формы входа
        [HttpPost]
        public ActionResult Enter(string login, string pin, string returnUrl)
        {
            var customer = _identityManager.ValidateCredentials(login, pin);
            if (customer != null)
            {
                CreateSessionForCustomer(customer);
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Nume de utilizator sau parolă incorecte.");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // GET: Страница регистрации
        public ActionResult CreateAccount()
        {
            return View();
        }

        // POST: Обработка формы регистрации
        [HttpPost]
        public ActionResult CreateAccount(string login, string pin)
        {
            var customer = _identityManager.CreateNewAccount(login, pin);
            if (customer != null)
            {
                CreateSessionForCustomer(customer);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Înregistrarea a eșuat. Vă rugăm să încercați din nou.");
            return View();
        }

        // GET: Выход из системы
        public ActionResult Exit()
        {
            FormsAuthentication.SignOut();
            var expiredCookie = new HttpCookie(FormsAuthentication.FormsCookieName, "")
            {
                Expires = DateTime.Now.AddYears(-1)
            };
            Response.Cookies.Add(expiredCookie);

            return RedirectToAction("Enter");
        }

        // GET: Альтернативная страница входа, которая будет вызываться из Cart и Review
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View("Enter");
        }

        private void CreateSessionForCustomer(Customer customer)
        {
            try
            {
                string roles = customer.AccessLevel;
                
                var customerRoles = _customerBL.GetCustomerRoles(customer.Id);
                if (customerRoles != null && customerRoles.Any())
                {
                    var roleNames = customerRoles.Select(r => r.Name);
                    roles = string.Join(",", roleNames);
                }
                
                var sessionTicket = new FormsAuthenticationTicket(
                    1,
                    customer.Username,
                    DateTime.Now,
                    DateTime.Now.AddMinutes(30),
                    true,
                    roles,
                    "/");

                var secureTicket = FormsAuthentication.Encrypt(sessionTicket);
                var sessionCookie = new HttpCookie(FormsAuthentication.FormsCookieName, secureTicket);
                Response.Cookies.Add(sessionCookie);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                (_identityManager as IDisposable)?.Dispose();
                (_customerBL as IDisposable)?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
    
    public class StaffOnlyAttribute : AuthorizeAttribute
    {
        private const string AllowedAccessLevel = "Admin";

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
                return false;
                
            return httpContext.User.IsInRole(AllowedAccessLevel);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary
                    {
                        { "controller", "Error" },
                        { "action", "AccessDenied" }
                    });
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
} 