#region

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Providers.Mobile;
using MyHerbalife3.Ordering.ViewModel.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyHerbalife3.Ordering.WebAPI.Attributes;

#endregion

namespace MyHerbalife3.Ordering.Controllers.Mobile
{
    [CustomResponseHeader]
    public class MobileOrderSummaryController : ApiController
    {
        internal IMobileOrderSummaryProvider _mobileSummaryProvider;

        /// <summary>
        ///     ctor
        /// </summary>
        public MobileOrderSummaryController()
        {
            _mobileSummaryProvider = new MobileOrderSummaryProvider();
        }

        /// <summary>
        ///     MobileOrderListViewProvider.GetOrderList
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [CamelCasingFilter]
        [SetCulture]
        [UserTokenAuthenticate]
        public MobileResponseWrapper Get([FromUri] OrderSummaryRequestViewModel request)
        {
            if (request == null)
            {
                throw CreateException(HttpStatusCode.BadRequest, "request is null", 500);
            }
            string obj = JsonConvert.SerializeObject(request);
            request.Locale = Thread.CurrentThread.CurrentCulture.Name;
            var mobileResponseWrapper =  new MobileResponseWrapper {Data = _mobileSummaryProvider.GetOrderList(request)};
            JObject json = JObject.Parse(obj);
            MobileActivityLogProvider.ActivityLog(json, mobileResponseWrapper, request.MemberId, true,
                        this.Request.RequestUri.ToString(),
                        this.Request.Headers.ToString(),
                        this.Request.Headers.UserAgent.ToString(),
                        request.Locale);

            return mobileResponseWrapper;
        }


        private HttpResponseException CreateException(HttpStatusCode httpStatusCode, string reasonText, int errorCode)
        {
            var error = new HttpError(reasonText) { { "code", errorCode } };
            return new HttpResponseException(Request.CreateErrorResponse(httpStatusCode, error));
        }

    }
}