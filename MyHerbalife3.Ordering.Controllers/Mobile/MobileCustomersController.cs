#region

using System.Web.Http;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Providers.Mobile;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.WebAPI.Attributes;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endregion

namespace MyHerbalife3.Ordering.Controllers.Mobile
{
    [CustomResponseHeader]
    public class MobileCustomersController : ApiController
    {
        internal IMobileCustomersProvider _mobileCustomersProvider;

        /// <summary>
        ///     ctor
        /// </summary>
        /// 
        public MobileCustomersController()
        {
            _mobileCustomersProvider = new MobileCustomersProvider();
        }

        /// <summary>
        ///     Get list of Preferred-Customer by member.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [CamelCasingFilter]
        [UserTokenAuthenticate]
        public MobileResponseWrapper Get([FromUri] PreferredCustomerRequestViewModel request)
        {
            try
            {
                string obj = JsonConvert.SerializeObject(request);
                var response = new MobileResponseWrapper
                {
                    Data = _mobileCustomersProvider.GetPreferredCustomers(request)
                };
                JObject json = JObject.Parse(obj);
                MobileActivityLogProvider.ActivityLog(json, response, request.MemberId, true,
                        this.Request.RequestUri.ToString(),
                        this.Request.Headers.ToString(),
                        this.Request.Headers.UserAgent.ToString(),
                        request.Locale);

                return response;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            throw new Exception("Method Get on MobileCustomersController not return values.");
        }
    }
}