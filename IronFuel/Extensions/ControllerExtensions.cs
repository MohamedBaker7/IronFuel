using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace IronFuel.Web.Extensions
{
    public static class ControllerExtensions
    {
        public static string RenderPartialViewToString(this Controller controller, string viewName, object model)
        {
            controller.ViewData.Model = model;

            using var sw = new StringWriter();
            var viewResult = controller.HttpContext.RequestServices
                .GetService<ICompositeViewEngine>()!
                .FindView(controller.ControllerContext, viewName, false);

            var viewContext = new ViewContext(
                controller.ControllerContext,
                viewResult.View!,
                controller.ViewData,
                controller.TempData,
                sw,
                new HtmlHelperOptions()
            );

            viewResult.View!.RenderAsync(viewContext).GetAwaiter().GetResult();
            return sw.ToString();
        }
    }
}
