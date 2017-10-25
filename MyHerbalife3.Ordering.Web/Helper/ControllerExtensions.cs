#region

using System.IO;
using System.Web.Mvc;

#endregion

namespace MyHerbalife3.Ordering.Web.Helper
{
    public static class ControllerExtensions
    {
        public static string RenderViewAsString(this Controller controller, string viewName, object model)
        {
            return RenderViewAsString(controller, viewName, new ViewDataDictionary(model));
        }

        public static string RenderViewAsString(this Controller controller, string viewName, ViewDataDictionary viewData)
        {
            var controllerContext = controller.ControllerContext;

            var viewResult = ViewEngines.Engines.FindView(controllerContext, viewName, null);

            StringWriter stringWriter;

            using (stringWriter = new StringWriter())
            {
                var viewContext = new ViewContext(
                    controllerContext,
                    viewResult.View,
                    viewData,
                    controllerContext.Controller.TempData,
                    stringWriter);

                viewResult.View.Render(viewContext, stringWriter);
                viewResult.ViewEngine.ReleaseView(controllerContext, viewResult.View);
            }

            return stringWriter.ToString();
        }
    }
}