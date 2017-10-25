#region
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.ViewModel.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyHerbalife3.Ordering.WebAPI.Attributes;
#endregion

namespace MyHerbalife3.Ordering.Controllers.Mobile
{
    public class MobileExpireCatalogCacheController : ApiController
    {
        internal IExpireCatalogCacheProvider _expireCatalogCacheProvider;

        /// <summary>
        ///     ctor
        /// </summary>
        public MobileExpireCatalogCacheController()
        {
            _expireCatalogCacheProvider = new ExpireCatalogCacheProvider();
        }

        /// <summary>
        ///     Get announcement data.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [CamelCasingFilter]
        [SetCulture]
        [UserTokenAuthenticate(UserIdValidation = false)]
        public MobileResponseWrapper Get([FromUri] ExpireCatalogCacheRequestViewModel request)
        {
            if (request == null)
            {
                throw CreateException(HttpStatusCode.BadRequest, "request is null", 500404);
            }
            request.Locale = Thread.CurrentThread.CurrentCulture.Name;

            var response = new MobileResponseWrapper
            {
                Data = _expireCatalogCacheProvider.ExpireCatalogCache(request.cacheName, request.inputCacheKey, request.Locale)
            };
            return response;
        }

        private HttpResponseException CreateException(HttpStatusCode httpStatusCode, string reasonText, int errorCode)
        {
            var error = new HttpError(reasonText) { { "code", errorCode } };
            return new HttpResponseException(Request.CreateErrorResponse(httpStatusCode, error));
        }
    }
}
