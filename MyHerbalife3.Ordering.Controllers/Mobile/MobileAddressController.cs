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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endregion

namespace MyHerbalife3.Ordering.Controllers.Mobile
{
    [CustomResponseHeader]
    public class MobileAddressController : ApiController
    {
        private readonly IMobileAddressProvider _iMobileAddressProvider;

        public MobileAddressController()
        {
            _iMobileAddressProvider = new MobileAddressProvider();
        }

        [HttpGet]
        [CamelCasingFilter]
        [SetCulture]
        [UserTokenAuthenticate]
        public MobileResponseWrapper Get([FromUri] GetAddressRequestViewModel request)
        {
            try
            {
                string obj = JsonConvert.SerializeObject(request);
                request.Locale = Thread.CurrentThread.CurrentCulture.Name;
                if (request == null || request.MemberId == null || string.IsNullOrEmpty(request.MemberId) ||
                    string.IsNullOrWhiteSpace(request.MemberId) ||
                    request.Locale == null || string.IsNullOrEmpty(request.Locale) ||
                    string.IsNullOrWhiteSpace(request.Locale))
                {
                    throw CreateException(HttpStatusCode.BadRequest, "Get Addresses incomplete/invalid information sended", 210416);
                }

                var result = _iMobileAddressProvider.Get(request);

                if (result != null)
                {
                    var response = new MobileResponseWrapper
                    {
                        Data = new AddressListResponseViewModel { Address = result },
                    };
                    JObject json = JObject.Parse(obj);
                    MobileActivityLogProvider.ActivityLog(json, response, request.MemberId, true, 
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
                throw CreateException(HttpStatusCode.InternalServerError, "Internal server errror searching for Get Address" + ex.Message , 166767);
            }

            throw CreateException(HttpStatusCode.InternalServerError, "Internal server errror searching for Get Address", 210416);
        }

        //save and update is the same method
        [HttpPost]
        [CamelCasingFilter]
        [SetCulture]
        [UserTokenAuthenticate]
        public SaveAddressResponseViewModel Post(AddressRequestViewModel request, string memberId)
        {
            try
            {
                string obj = JsonConvert.SerializeObject(request);
                //added country to this validation, because Country field is required to retrieve the address list
                if (request == null || request.Data == null || request.Data.Country == null)
                {
                    //200423 Address is readonly
                    //200404 404 Customer associated to the address does not exist
                    throw CreateException(HttpStatusCode.BadRequest, "Invalid or Incomplete Address information (Null Data)", 200416);

                }
                var locale = Thread.CurrentThread.CurrentCulture.Name;
                var address = request.Data;
                var result = _iMobileAddressProvider.Save(ref address, memberId, locale);

                if (result == -3 || result == 0)
                {
                    //200422 422 Address Validation errors occurred
                    return new SaveAddressResponseViewModel
                    {
                        Data = request.Data,
                        Error =
                            new ErrorViewModel
                            {
                                Code = 200422,
                                Message = "Address Validation errors occurred"
                            }
                    };
                }

                var response = new SaveAddressResponseViewModel {Data = address, Error = null};
                JObject json = JObject.Parse(obj);
                MobileActivityLogProvider.ActivityLog(json, response, memberId, true,
                        this.Request.RequestUri.ToString(),
                        this.Request.Headers.ToString(),
                        this.Request.Headers.UserAgent.ToString(),
                        locale);

                return response;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
                throw CreateException(HttpStatusCode.InternalServerError,
                    "Internal server errror for Save Address" + ex.Message, 200416);
            }
            return null;
        }

        [HttpDelete]
        [CamelCasingFilter]
        [SetCulture]
        [UserTokenAuthenticate]
        public AddressViewModel Delete(Guid id, string memberId)
        {
            try
            {
                var locale = Thread.CurrentThread.CurrentCulture.Name;
                var result= _iMobileAddressProvider.Delete(memberId, id, locale);
                if (null == result)
                {
                    throw CreateException(HttpStatusCode.NotFound,
                        "Customer associated to the address does not exist", 220404);
                }
                return result;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
                throw CreateException(HttpStatusCode.InternalServerError,
                    "Internal server errror for Save Address" + ex.Message, 200416);
            }
        }


        private HttpResponseException CreateException(HttpStatusCode httpStatusCode, string reasonText, int errorCode)
        {
            var error = new HttpError(reasonText) { { "code", errorCode } };
            return new HttpResponseException(Request.CreateErrorResponse(httpStatusCode, error));
        }


    }
}