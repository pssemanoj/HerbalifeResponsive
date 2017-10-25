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
using MyHerbalife3.Ordering.WebAPI.Attributes;

#endregion

namespace MyHerbalife3.Ordering.Controllers.Mobile
{
    [CustomResponseHeader]
    public class MobileShippingMethodsController : ApiController
    {
        private readonly IMobileAddressProvider _iMobileAddressProvider;

        public MobileShippingMethodsController()
        {
            _iMobileAddressProvider = new MobileAddressProvider();
        }

        [HttpGet]
        [CamelCasingFilter]
        [SetCulture]
        [UserTokenAuthenticate]
        public MobileResponseWrapper Get(Guid id, string memberId)
        {
            try
            {
                var locale = Thread.CurrentThread.CurrentCulture.Name;
                if (id == null || string.IsNullOrEmpty(memberId))
                {
                    throw CreateException(HttpStatusCode.BadRequest, "Get Addresses incomplete/invalid information sended", 210416);
                }

                var result = _iMobileAddressProvider.GetShippingMethods(memberId, id, locale);

                if (result != null)
                {
                    var mobileResponseWrapper =  new MobileResponseWrapper
                    {
                        Data = new ShippingMethodsResponseViewModel { ShippingMethods = result },
                    };

                    MobileActivityLogProvider.ActivityLog(string.Empty, mobileResponseWrapper, memberId, true,
                        this.Request.RequestUri.ToString(),
                        this.Request.Headers.ToString(),
                        this.Request.Headers.UserAgent.ToString(),
                        locale);

                    return mobileResponseWrapper;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
                throw CreateException(HttpStatusCode.InternalServerError, "Internal server errror searching for Get shippingMethods" + ex.Message, 166767);
            }

            throw CreateException(HttpStatusCode.InternalServerError, "Internal server errror searching for Get shippingMethods", 210416);
        }

        private HttpResponseException CreateException(HttpStatusCode httpStatusCode, string reasonText, int errorCode)
        {
            var error = new HttpError(reasonText) { { "code", errorCode } };
            return new HttpResponseException(Request.CreateErrorResponse(httpStatusCode, error));
        }
    }
}