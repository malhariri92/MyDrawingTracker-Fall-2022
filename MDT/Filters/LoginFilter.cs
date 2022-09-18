using MDT.Models;
using MDT.Models.DTO;
using MDT.ViewModels;
using System.Web.Mvc;
using System.Web.Mvc.Filters;

namespace MDT.Filters
{
    /// <summary>
    /// Filter for establishing user identity. Should be applied to all controllers.
    /// </summary>
    public class LoginFilter : ActionFilterAttribute, IAuthenticationFilter
    {
        /// <summary>
        /// Establishes user identity for AD users.
        /// </summary>
        /// <param name="filterContext"></param>
        public void OnAuthentication(AuthenticationContext filterContext)
        {
            UserDTO user = (UserDTO)filterContext.HttpContext.Session["User"];

            if (user == null )
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
            else
            {
                filterContext.HttpContext.Session["User"] = WebManager.GetUserDTO(user.UserId);
                filterContext.HttpContext.Session["Group"] = WebManager.GetGroupDTO(user.CurrentGroupId);
                filterContext.HttpContext.Session["IsAdmin"] = WebManager.IsGroupAdmin(user.CurrentGroupId, user.UserId);
            }
        }

        /// <summary>
        /// Redirects Non-AD Users to the Login view on the Users controller. 
        /// NOTE: Users controller has not been implemented yet.
        /// </summary>
        /// <param name="filterContext"></param>
        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            if (filterContext.Result == null || filterContext.Result is HttpUnauthorizedResult)
            {
                filterContext.Result = new RedirectResult("~/Home/Nope");
            }
        }
    }
}