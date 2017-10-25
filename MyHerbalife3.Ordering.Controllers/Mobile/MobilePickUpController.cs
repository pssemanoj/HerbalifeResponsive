#region

using System;
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
    public class MobilePickUpController : ApiController
    {
        private readonly IMobilePickUpProvider _iMobilePickUpProvider;

        public MobilePickUpController()
        {
            _iMobilePickUpProvider = new MobilePickUpProvider();
        }

        //Get the delivwery info list
        [HttpGet]
        [CamelCasingFilter]
        [SetCulture]
        [UserTokenAuthenticate]
        public MobileResponseWrapper Get([FromUri] PickUpRequestViewModel request)
        {
            if (request == null)
            {
                throw CreateException(HttpStatusCode.BadRequest, "Location not found, request Null", "300404");
            }
            string obj = JsonConvert.SerializeObject(request);
            request.Locale = Thread.CurrentThread.CurrentCulture.Name;
            var result = _iMobilePickUpProvider.GetDeliveryOptions(request.Locale,
                new AddressViewModel
                {
                    StateProvinceTerritory = request.State,
                    City = request.City,
                    Country = request.Country,
                    PostalCode = request.PostalCode,
                    CountyDistrict = request.District
                });

            if (result != null)
            {
                if (request.City != null && !string.IsNullOrEmpty(request.City) &&
                    !string.IsNullOrWhiteSpace(request.City))
                {
                    result = result.FindAll(x => !string.IsNullOrEmpty(x.Address.City) ? x.Address.City.Trim() == request.City :
                        x.Address.City == request.City);
                }
                if (request.State != null && !string.IsNullOrEmpty(request.State) &&
                    !string.IsNullOrWhiteSpace(request.State))
                {
                    result = result.FindAll(x => !string.IsNullOrEmpty(x.Address.StateProvinceTerritory) ?  x.Address.StateProvinceTerritory.Trim() == request.State :
                        x.Address.StateProvinceTerritory== request.State);
                }
                if (request.District != null && !string.IsNullOrEmpty(request.District) &&
                    !string.IsNullOrWhiteSpace(request.District))
                {
                    result = result.FindAll(x => !string.IsNullOrEmpty(x.Address.CountyDistrict) ? x.Address.CountyDistrict.Trim() == request.District :
                        x.Address.CountyDistrict == request.District);
                }

                var mobileListResponseWrapper = new MobileResponseWrapper
                {
                    Data = new PickUpListResponseViewModel {Pickup = result}
                };
                JObject json = JObject.Parse(obj);
                MobileActivityLogProvider.ActivityLog(json, mobileListResponseWrapper, request.MemberId, true,
                        this.Request.RequestUri.ToString(),
                        this.Request.Headers.ToString(),
                        this.Request.Headers.UserAgent.ToString(),
                        request.Locale);

                return mobileListResponseWrapper;
            }

            throw CreateException(HttpStatusCode.InternalServerError, "Location not found", "300404");
        }


        [HttpOptions]
        [CamelCasingFilter]
        [SetCulture]
        [UserTokenAuthenticate]
        public MobileResponseWrapper GetSavedOptions([FromUri] PickUpRequestViewModel request)
        {
            if (request == null)
            {
                throw CreateException(HttpStatusCode.BadRequest, "Location not found, request Null", "300404");
            }
            string obj = JsonConvert.SerializeObject(request);
            request.Locale = Thread.CurrentThread.CurrentCulture.Name;
            var result = _iMobilePickUpProvider.GetSavedDeliveryOptions(request.MemberId, request.Locale);

            if (result != null)
            {
                var mobileResponseWrapper = new MobileResponseWrapper
                {
                    Data = new PickUpListResponseViewModel {Pickup = result}
                };
                JObject json = JObject.Parse(obj);
                MobileActivityLogProvider.ActivityLog(json, mobileResponseWrapper, request.MemberId, true,
                        this.Request.RequestUri.ToString(),
                        this.Request.Headers.ToString(),
                        this.Request.Headers.UserAgent.ToString(),
                        request.Locale);

                return mobileResponseWrapper;

            }
            return new MobileResponseWrapper
            {
                Data = null,
                //Error = new ErrorViewModel {Code = "0404", Message = "No Data Available"}
            };
        }


        //save the pu preference
        [HttpPost]
        [SetCulture]
        [UserTokenAuthenticate]
        public PickUpResponseViewModel Post(ModifyPickUpRequestViewModel request)
        {
            if (request == null)
            {
                throw CreateException(HttpStatusCode.BadRequest, "Location not found, request Null", "300404");
            }
            string obj = JsonConvert.SerializeObject(request);
            request.Locale = Thread.CurrentThread.CurrentCulture.Name;
            var result = _iMobilePickUpProvider.SavePickUpPreference(request.MemberId, request.PickUpModel,
                request.Locale);
            if (result != null)
            {
                if (result.IDSaved == -2)
                {
                    //return an error duplicate nick name / Alias
                    return new PickUpResponseViewModel
                    {
                        Data = null,
                        Error =
                            new ErrorViewModel
                            {
                                Code = 0416,
                                Message =
                                    "Invalid or Incomplete Location information: Pick Up Location Alias Duplicated"
                            }
                    };
                }
                if (result.IDSaved == -3)
                {
                    //return an error, PU location saved previousli
                    return new PickUpResponseViewModel
                    {
                        Data = null,
                        Error =
                            new ErrorViewModel
                            {
                                Code = 0416,
                                Message =
                                    "Invalid or Incomplete Location information : Pick Up Location previously saved"
                            }
                    };
                }

                var pickupResponseViewModel = new PickUpResponseViewModel {Data = result, Error = null};
                JObject json = JObject.Parse(obj);
                MobileActivityLogProvider.ActivityLog(json, pickupResponseViewModel, request.MemberId, true,
                        this.Request.RequestUri.ToString(),
                        this.Request.Headers.ToString(),
                        this.Request.Headers.UserAgent.ToString(),
                        request.Locale);

                return pickupResponseViewModel;
            }

            throw CreateException(HttpStatusCode.InternalServerError, "Invalid or Incomplete Location information: Error Occurs", "300416");
            
        }


        [HttpDelete]
        [SetCulture]
        [UserTokenAuthenticate]
        public bool Delete(ModifyPickUpRequestViewModel request)
        {
            if (request == null)
            {
                throw CreateException(HttpStatusCode.BadRequest, "Location not found, request Null", "300404");
            }
            try
            {
                request.Locale = Thread.CurrentThread.CurrentCulture.Name;
                return _iMobilePickUpProvider.DeletePickUpLocation(request.MemberId, request.PickUpModel, request.Locale);
            }
            catch (Exception ex)
            {
                throw CreateException(HttpStatusCode.InternalServerError, "Internal Server Error", "300404");
            }
            
        }

        private HttpResponseException CreateException(HttpStatusCode httpStatusCode, string reasonText, string errorCode)
        {
            var error = new HttpError(reasonText) { { "code", errorCode } };
            return new HttpResponseException(Request.CreateErrorResponse(httpStatusCode, error));
        }

    }
}