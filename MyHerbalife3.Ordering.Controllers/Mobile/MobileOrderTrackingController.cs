#region

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using HL.Common.Logging;
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
    public class MobileOrderTrackingController : ApiController
    {
        private readonly IMobileOrderTrackingProvider _iMobileOrderTrackingProvider;

        public MobileOrderTrackingController()
        {
            _iMobileOrderTrackingProvider = new MobileOrderTrackingProvider();
        }

        [HttpGet]
        [CamelCasingFilter]
        [SetCulture]
        [UserTokenAuthenticate]
        public MobileResponseWrapper Get([FromUri] OrderTrackingRequestViewModel request)
        {
            try
            {
                request.Locale = Thread.CurrentThread.CurrentCulture.Name;
                if (request == null || request.MemberId == null || string.IsNullOrEmpty(request.MemberId) ||
                    string.IsNullOrWhiteSpace(request.MemberId) ||
                    request.Locale == null || string.IsNullOrEmpty(request.Locale) ||
                    string.IsNullOrWhiteSpace(request.Locale))
                {
                    throw CreateException(HttpStatusCode.BadRequest, "Get Order Tracking incomplete/invalid information sended", 210416);
                }
                string obj = JsonConvert.SerializeObject(request);
                var result = _iMobileOrderTrackingProvider.Get(request);

                if (result != null)
                {
                    var response = new MobileResponseWrapper
                    {
                        Data =
                            new OrderTrackingResponseViewModel
                            {
                                ExpressInfo = result,
                                Locale = request.Locale,
                                OrderId = request.OrderId,
                                MemberId = request.MemberId
                            }
                    };
                    
                    Object json = JObject.Parse(obj);
                    MobileActivityLogProvider.ActivityLog(json, string.Empty, request.MemberId, true,
                        this.Request.RequestUri.ToString(),
                        this.Request.Headers.ToString(),
                        this.Request.Headers.UserAgent.ToString(),
                        request.Locale);

                    return response;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
                throw CreateException(HttpStatusCode.InternalServerError, "Internal server errror searching for Get Order Tracking Express Info" + ex.Message, 166767);
            }

            throw CreateException(HttpStatusCode.InternalServerError, "Internal server errror searching for Get Order Tracking Express Info", 210416);
        }

        

        private HttpResponseException CreateException(HttpStatusCode httpStatusCode, string reasonText, int errorCode)
        {
            var error = new HttpError(reasonText) { { "code", errorCode } };
            return new HttpResponseException(Request.CreateErrorResponse(httpStatusCode, error));
        }


    }
}