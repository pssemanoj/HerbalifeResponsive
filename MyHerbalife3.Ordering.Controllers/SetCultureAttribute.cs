#region

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

#endregion

namespace MyHerbalife3.Ordering.Controllers
{
    public class SetCultureAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            try
            {
                if (null == filterContext || null == filterContext.Request ||
                    null == filterContext.Request.Headers)
                    return;
                var headerValues = filterContext.Request.Headers.GetValues("X-HLLocale");
                var locale = headerValues.FirstOrDefault();

                if (locale == "en-CN")
                {
                    locale = "zh-CN";
                }

                Thread.CurrentThread.CurrentCulture = new CultureInfo(locale);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(locale);
            }
            catch (Exception exception)
            {
                Trace.TraceError("SetCultureAttribute can't resolve locale.\n{0}", exception);
            }
        }
    }
}