using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace CosmeticShop
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            try
            {
                var authCookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (authCookie == null) return;
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                if (authTicket == null || authTicket.Expired) return;
                var roles = authTicket.UserData.Split(',');
                HttpContext.Current.User = new GenericPrincipal(new FormsIdentity(authTicket), roles);
            }
            catch
            {
                FormsAuthentication.SignOut();
                var authCookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (authCookie == null) return;
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                if (authTicket == null || authTicket.Expired) return;
                var roles = authTicket.UserData.Split(',');
                HttpContext.Current.User = new GenericPrincipal(new FormsIdentity(authTicket), roles);
            }
        }
        
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}