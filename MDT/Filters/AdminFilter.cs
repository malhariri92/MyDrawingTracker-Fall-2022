using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace MDT.Filters
{
    public class AdminFilter : AuthorizeAttribute
    {
        public string Role;
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {

            GenericPrincipal principal = (GenericPrincipal)httpContext.Session["Ident"];
            return principal.IsInRole(Role) || principal.IsInRole("Site Admin");

        }

        
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("~/Home/Index");
        }
    }
}