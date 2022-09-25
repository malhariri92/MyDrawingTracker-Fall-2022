using MDT.Models;
using MDT.Models.DTO;
using System.Security.Principal;
using System.Web.Mvc;
using System.Web.Mvc.Filters;

namespace MDT.Filters
{
    public class LoginFilter : ActionFilterAttribute, IAuthenticationFilter
    {

        public void OnAuthentication(AuthenticationContext filterContext)
        {
            UserDTO user = (UserDTO)filterContext.HttpContext.Session["User"];


            if (user == null)
            {
                filterContext.HttpContext.Session["RedirectUrl"] = filterContext.HttpContext.Request.RawUrl;
                filterContext.Result = new HttpUnauthorizedResult();

            }
            else
            {
                GenericPrincipal principal = (GenericPrincipal)filterContext.HttpContext.Session["Ident"];
                filterContext.HttpContext.User = principal;
                filterContext.HttpContext.Session["User"] = WebManager.GetUserDTO(user.UserId);
                filterContext.HttpContext.Session["Group"] = WebManager.GetGroupDTO(user.CurrentGroupId);
            }
        }


        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            if (filterContext.Result == null || filterContext.Result is HttpUnauthorizedResult)
            {
                filterContext.Result = new RedirectResult("~/Home/Index");
            }
        }
    }
}