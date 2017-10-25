using System;
using System.Web.Http.Filters;

namespace MyHerbalife3.Ordering.WebAPI.Attributes
{
    public class CustomResponseHeaderAttribute : ActionFilterAttribute
    {
        private const string DateTimeFormat = "yyyy-MM-ddTHH:mm:sszzz";

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            actionExecutedContext.Response.Content.Headers.Add(
                Constants.ResponseHeaders.ServerDate,
                DateTime.Now.ToString(DateTimeFormat));

            actionExecutedContext.Response.Content.Headers.Add(
                Constants.ResponseHeaders.ServerHost,
                Environment.MachineName);
        }
    }
}