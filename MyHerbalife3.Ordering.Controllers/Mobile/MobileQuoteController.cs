#region

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Providers.Mobile;
using MyHerbalife3.Ordering.ViewModel.Model;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using MyHerbalife3.Ordering.WebAPI.Attributes;

#endregion

namespace MyHerbalife3.Ordering.Controllers.Mobile
{
    [CustomResponseHeader]
    public class MobileQuoteController : ApiController
    {
        internal IMobileQuoteProvider _iMobileQuoteProvider;
        internal MobileQuoteHelper _mobileQuoteHelper;


        public MobileQuoteController()
        {
            _mobileQuoteHelper = new MobileQuoteHelper();
            _iMobileQuoteProvider = new MobileQuoteProvider(new MobileDualOrderMonthProvider(), _mobileQuoteHelper);
        }

        /// <summary>
        ///     creates the Quote
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        [CamelCasingFilter]
        [SetCulture]
        [UserTokenAuthenticate]
        public MobileResponseWrapper Put(OrderRequestViewModel request)
        {
            if (request == null)
            {
                throw CreateException(HttpStatusCode.BadRequest, "request is null", 166767);
            }
            string obj = JsonConvert.SerializeObject(request);
            SetOrderMemberId(request.Data);
            request.Data.Locale = Thread.CurrentThread.CurrentCulture.Name;
            var orderNumber = string.Empty;
            var isOrderSubmitted = false;
            if (_mobileQuoteHelper.CheckIfAnyOrderInProcessing(request.Data.MemberId, request.Data.Locale, ref orderNumber, ref isOrderSubmitted, request.Data.OrderNumber))
            {
                throw CreateException(HttpStatusCode.NotAcceptable,
                    "order still processing", 110406, orderNumber);
            }

            //If SR Pricing an order for PC then check whether the PC is expired or not.
            if (request.Data.Locale == "zh-CN" && !string.IsNullOrEmpty(request.Data.CustomerId) && request.Data.CustomerId != request.Data.MemberId && _mobileQuoteHelper.IsPCExpired(request))
            {
                throw CreateException(HttpStatusCode.NotAcceptable,
                    "PC membership is expired", 110421);
            }

            List<ValidationErrorViewModel> errors = null;
            var data = _iMobileQuoteProvider.Quote(request.Data, ref errors);
            var mobileResponseWrapper =  new MobileResponseWrapper
            {
                Data = data,
                ValidationErrors = null != errors && errors.Any() ? errors : null
            };
            JObject json = JObject.Parse(obj);
            MobileActivityLogProvider.ActivityLog(json, mobileResponseWrapper, request.Data.MemberId, true,
                        this.Request.RequestUri.ToString(),
                        this.Request.Headers.ToString(),
                        this.Request.Headers.UserAgent.ToString(),
                        request.Data.Locale);

            return mobileResponseWrapper;
        }

        #region private methods

        private static void SetOrderMemberId(OrderViewModel request)
        {
            //validating if is a customer order
            if (!string.IsNullOrEmpty(request.CustomerId))
            {
                //set the cust id as memberid
                request.OrderMemberId = request.CustomerId;
            }
            else
            {
                request.OrderMemberId = request.MemberId;
            }
        }

        private HttpResponseException CreateException(HttpStatusCode httpStatusCode, string reasonText, int errorCode)
        {
            var error = new HttpError(reasonText) {{"code", errorCode}};
            return new HttpResponseException(Request.CreateErrorResponse(httpStatusCode, error));
        }
        private HttpResponseException CreateException(HttpStatusCode httpStatusCode, string reasonText, int errorCode, string orderNumber)
        {
            var error = new HttpError(reasonText) { { "code", errorCode }, {"data", new {orderNumber}} };
            return new HttpResponseException(Request.CreateErrorResponse(httpStatusCode, error));
        }
        #endregion
    }
}