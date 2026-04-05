using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace IronFuel.Web.Filter
{

    public class AjaxOnlyAttribute : ActionMethodSelectorAttribute
    {
        public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
        {
            var Request = routeContext.HttpContext.Request;
            var isAjax = Request.Headers["x-requested-with"] == "XMLHttpRequest";

            return isAjax;
        }
    }

}
