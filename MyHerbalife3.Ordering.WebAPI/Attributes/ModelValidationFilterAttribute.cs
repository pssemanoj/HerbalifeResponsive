using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;
using HL.Common.Logging;
using MyHerbalife3.Ordering.WebAPI.Model;

namespace MyHerbalife3.Ordering.WebAPI.Attributes
{
    public class ModelValidationFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext.ModelState.IsValid == false)
            {
                // Return the validation errors in the response body.
                var validationErrors = new Dictionary<string, IEnumerable<string>>();
                foreach (KeyValuePair<string, ModelState> keyValue in actionContext.ModelState)
                {
                    validationErrors[keyValue.Key] = keyValue.Value.Errors.Select(e => e.ErrorMessage);
                }
                LoggerHelper.Warn("Validation Errors: \n" + validationErrors);

                var errorResponse = Error.HTTPBadRequest;
                actionContext.Response = new HttpResponseMessage(errorResponse.StatusCode)
                {
                    RequestMessage = actionContext.Request,
                    Content = new ObjectContent<Error>(errorResponse, new JsonMediaTypeFormatter())
                };


            }
        }
    }
}