using MDT.Models.DTO;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace MDT.Filters
{
    public class VerifiedFilter : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            UserDTO user = (UserDTO)httpContext.Session["User"];
            GroupDTO group = (GroupDTO)httpContext.Session["Group"];
            httpContext.Session["RedirectUrl"] = httpContext.Request.RawUrl;
            httpContext.Session["VerifiedUser"] = user.IsVerified;
            httpContext.Session["ApprovedGroup"] = (group.IsApproved ?? false);
            return user.IsVerified && (group.IsApproved ?? false);
        }

        
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            
            filterContext.Result = new RedirectResult("~/Home/Index");
        }
    }
}