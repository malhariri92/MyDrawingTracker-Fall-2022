using MDT.Controllers;
using System;
using System.Web.Mvc;
using System.Web.Mvc.Filters;

namespace MDT.Filters
{
    public class SetupFilter : ActionFilterAttribute, IActionFilter
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ((BaseController)filterContext.Controller).Setup();
        }
    }
}
