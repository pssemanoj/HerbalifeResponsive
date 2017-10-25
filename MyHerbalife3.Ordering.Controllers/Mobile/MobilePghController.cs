#region

using System;
using System.Collections.Generic;
using System.Web.Http;
using MyHerbalife3.Ordering.Providers.Mobile;
using MyHerbalife3.Ordering.ViewModel.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyHerbalife3.Ordering.WebAPI.Attributes;
using System.Linq;

#endregion

namespace MyHerbalife3.Ordering.Controllers.Mobile
{
    [CustomResponseHeader]
    public class MobilePghController : ApiController
    {
        [HttpPost]
        [CamelCasingFilter]
        [SetCulture]
        [UserTokenAuthenticate]
        public MobilePghResponseWrapper InsertPghOrder(MobilePghOrderRequestViewModel request)
        {
            try
            {
                var response = new MobilePghResponseWrapper();

                if (WebAPI.Provider.SSO.SSOProvider.UseSSO(request.Data.Locale, Getclient()))
                {
                    if (WebAPI.Provider.SSO.SSOProvider.IsValidUser(request.Data.DistributorId))
                    {
                        string distributor = WebAPI.Provider.SSO.SSOProvider.CurrentDistributor(GetToken());
                        if (!string.IsNullOrEmpty(distributor)) request.Data.DistributorId = distributor;
                        else return response;
                    }
                }
              
                List<ValidationErrorViewModel> Errors = null;

                var orderResponse = MobilePghProvider.InsertPghOrder(request, out Errors);

                response.Data = orderResponse;
                response.ValidationErrors = Errors;

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private string Getclient()
        {
            string client = string.Empty;
            try
            {
                IEnumerable<string> headerValues = this.Request.Headers.GetValues("X-HLCLIENT");

                if (headerValues.Count() > 0)
                {
                    var clientQuery = headerValues.FirstOrDefault();
                    var clientValues = System.Web.HttpUtility.ParseQueryString(clientQuery);
                    client = clientValues["name"];
                }
            }
            catch
            {
                client = string.Empty;
            }
            return client;
        }

        private string GetToken()
        {
            string token = string.Empty;
            try
            {
                IEnumerable<string> headerValues = this.Request.Headers.GetValues("X-HLAUTH");

                if (headerValues.Count() > 0)
                    token = headerValues.FirstOrDefault();                
            }
            catch
            {
                token = string.Empty;
            }
            return token;
        }

    }
}