#region

using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Filters;
using Newtonsoft.Json.Serialization;

#endregion

namespace MyHerbalife3.Ordering.Controllers
{
    public class CamelCasingFilterAttribute : ActionFilterAttribute
    {
        private static readonly JsonMediaTypeFormatter _camelCasingFormatter = new JsonMediaTypeFormatter();

        static CamelCasingFilterAttribute()
        {
            _camelCasingFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var content = actionExecutedContext.Response.Content as ObjectContent;
            if (content == null)
            {
                return;
            }
            if (content.Formatter is JsonMediaTypeFormatter)
            {
                actionExecutedContext.Response.Content = new ObjectContent(content.ObjectType, content.Value,
                    _camelCasingFormatter);
            }
        }
    }
}