#region

using System;
using System.Web.Http;
using MyHerbalife3.Ordering.WebAPI.Attributes;
using MyHerbalife3.Ordering.Providers.Mobile;
using MyHerbalife3.Ordering.ViewModel.Model;

#endregion

namespace MyHerbalife3.Ordering.Controllers.Mobile
{
    [CustomResponseHeader]
    public class MobileSandboxController : ApiController
    {
        [HttpPut]
        [CamelCasingFilter]
        [SetCulture]
        [UserTokenAuthenticate(UserIdValidation = false)]
        public MobileSandboxResponseWrapper Put(MobileSandboxRequestViewModel parameters)
        {
            try
            {

                var sandboxEvents = MobileActivityLogProvider.GetSandboxEvents(parameters.Page, parameters.PageSize, parameters.DistributorId, parameters.Locale, parameters.AppName, parameters.StartDate, parameters.EndDate);

                var response = new MobileSandboxResponseWrapper
                {
                    Parameters = parameters,
                    SandboxEvents = sandboxEvents
                };

                return response;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }
    }
}