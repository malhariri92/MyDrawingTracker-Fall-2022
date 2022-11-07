using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using MDT.Models;
using MDT.Models.DTO;

namespace MDT.Filters
{
    public class AdminFilter : AuthorizeAttribute
    {
        public string Role;
        public string Permission;
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            UserDTO user = (UserDTO)httpContext.Session["User"];
            GenericPrincipal principal = (GenericPrincipal)httpContext.Session["Ident"];
            return principal.IsInRole(Role) || principal.IsInRole("Site Admin") || WebManager.HasPermission(user.CurrentGroupId, user.UserId, Permission);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("~/Home/Index");
        }
    }
}