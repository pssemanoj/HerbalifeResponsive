using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers.Mobile;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Shared.WebAPI.Attributes;

namespace MyHerbalife3.Ordering.Controllers.Mobile
{
    [CustomResponseHeader]
    public class MobileGetDiscontinuedSkuController:ApiController
    {
        private readonly MobileGetDisconinuedSkuProvider _getDisconinuedSkuProvider;

        public MobileGetDiscontinuedSkuController()
        {
            _getDisconinuedSkuProvider=new MobileGetDisconinuedSkuProvider();
        }

        [HttpPut]
        [CamelCasingFilter]
        [SetCulture]
        [UserTokenAuthenticate]
        public MobileResponseWrapper Get([FromUri] DiscontinuedSkuItemViewModel query)
        {
            
            try
            {
                
                if (query == null)
                {
                    throw CreateException(HttpStatusCode.BadRequest, "Invalid or Incomplete GetDiscontinuedSku information",
                        999999);
                }
                if (string.IsNullOrEmpty(query.ShoppingCartId) && (query.OrderId==Guid.Empty || string.IsNullOrEmpty(query.OrderNumber)))
                {
                    return new MobileResponseWrapper
                    {
                        Data = new DiscontinuedSkuResponseViewModel { DiscontinuedSkus = null, RecordCount = 0 }
                    };
                }
                query.Locale = Thread.CurrentThread.CurrentCulture.Name;

                GetDiscontinuedSkuParam request = new GetDiscontinuedSkuParam
                {
                    DistributorId = query.DistributorId,
                    Locale = query.Locale,
                };
                var result = _getDisconinuedSkuProvider.GetDiscontinuedSkuRequest(request);

                if (result != null)
                {
                    var response = new MobileResponseWrapper
                    {
                        Data =
                            new DiscontinuedSkuResponseViewModel
                            {
                                DiscontinuedSkus = result,
                                RecordCount = result.Count
                            }
                    };

                    MobileActivityLogProvider.ActivityLog(query, response, query.DistributorId, true,
                        Request.RequestUri.ToString(),
                        Request.Headers.ToString(),
                        Request.Headers.UserAgent.ToString(),
                        query.Locale);

                    return response;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
                throw CreateException(HttpStatusCode.InternalServerError,
                    "Internal server errror searching for Get Discontinued SKUs" + ex.Message, 404);
            }
            var responseWrapper = new MobileResponseWrapper
            {
                Data = new DiscontinuedSkuResponseViewModel { DiscontinuedSkus = null, RecordCount = 0 }
            };

            MobileActivityLogProvider.ActivityLog(query, responseWrapper, query.DistributorId, true,
                        Request.RequestUri.ToString(),
                        Request.Headers.ToString(),
                        Request.Headers.UserAgent.ToString(),
                        query.Locale);

            return responseWrapper;
        }

        private HttpResponseException CreateException(HttpStatusCode httpStatusCode, string reasonText, int errorCode)
        {
            var error = new HttpError(reasonText) { { "code", errorCode } };
            return new HttpResponseException(Request.CreateErrorResponse(httpStatusCode, error));
        }
    }
}
