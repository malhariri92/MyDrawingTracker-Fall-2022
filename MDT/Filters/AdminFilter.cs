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
            return principal.IsInRole(Role);

        }

        
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new PartialViewResult { ViewName = "Nope" };
            }
            else
            {
                filterContext.Result = new ViewResult { ViewName = "Nope" };
            }
        }
    }
}