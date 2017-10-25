#region

using System;
using System.Collections.Generic;
using System.Linq;
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
    public class MobileOrderController : ApiController
    {
        private readonly IMobileOrderProvider _iMobileOrderProvider;
        private readonly IMobileOrderSummaryProvider _mobileOrderSummaryProvider;
        private readonly MobileQuoteHelper _mobileQuoteHelper;

        public MobileOrderController()
        {
            _mobileQuoteHelper = new MobileQuoteHelper();
            _iMobileOrderProvider = new MobileOrderProvider(_mobileQuoteHelper, new MobileWechatHelper());
            _mobileOrderSummaryProvider = new MobileOrderSummaryProvider();
        }

        /// <summary>
        ///     Gets the orders for the query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [CamelCasingFilter]
        [SetCulture]
        [UserTokenAuthenticate]
        public MobileResponseWrapper Get([FromUri] OrderSearchParameter query)
        {
            try
            {
                if (query == null)
                {
                    throw CreateException(HttpStatusCode.BadRequest, "Invalid or Incomplete Order information", 101416);
                }
                string obj = JsonConvert.SerializeObject(query);
                query.Locale = Thread.CurrentThread.CurrentCulture.Name;

                var result = _mobileOrderSummaryProvider.GetOrders(query);
                if (result != null)
                {
                    if (!string.IsNullOrEmpty(query.OrderNumber))
                    {
                        result = result.FindAll(r => r.OrderNumber == query.OrderNumber);
                    }
                    var response = new MobileResponseWrapper
                    {
                        Data =
                            new OrdersResponseViewModel
                            {
                                Orders = result,
                                RecordCount = result != null && result.Any() ? result.Count : 0
                            }
                    };
                    JObject json = JObject.Parse(obj);
                    MobileActivityLogProvider.ActivityLog(json, response, query.MemberId, true,
                       this.Request.RequestUri.ToString(),
                       this.Request.Headers.ToString(),
                       this.Request.Headers.UserAgent.ToString(),
                       query.Locale);

                    return response;
                }

                if (!string.IsNullOrEmpty(query.OrderNumber))
                {
                    var cartId = 0;
                    var orderViewModel = _iMobileOrderProvider.GetOrderByOrderNumber(query.OrderNumber, query.MemberId,
                        query.Locale, ref cartId);
                  
                    if (null != orderViewModel)
                    {
                        var mobResponseWrapper = new MobileResponseWrapper
                        {
                            Data =
                                new OrdersResponseViewModel
                                {
                                    Orders = new List<OrderViewModel> {orderViewModel},
                                    RecordCount = 1
                                }
                        };
                        JObject json1 = JObject.Parse(obj);
                        MobileActivityLogProvider.ActivityLog(json1, mobResponseWrapper, query.MemberId, true,
                        this.Request.RequestUri.ToString(),
                        this.Request.Headers.ToString(),
                        this.Request.Headers.UserAgent.ToString(),
                        query.Locale);

                        return mobResponseWrapper;
                    }
                }                
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
                throw CreateException(HttpStatusCode.InternalServerError,
                    "Internal server errror searching for Get Orders" + ex.Message, 404);
            }
            var responseWrapper =  new MobileResponseWrapper
            {
                Data = new OrdersResponseViewModel {Orders = null, RecordCount = 0}
            };

            MobileActivityLogProvider.ActivityLog(query, responseWrapper, query.MemberId, true,
                        this.Request.RequestUri.ToString(),
                        this.Request.Headers.ToString(),
                        this.Request.Headers.UserAgent.ToString(),
                        query.Locale);

            return responseWrapper;
        }

        /// <summary>
        ///     Below controller method is used for create/update order
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [CamelCasingFilter]
        [SetCulture]
        [UserTokenAuthenticate]
        public OrderResponseViewModel Post([FromBody] OrderRequestViewModel request)
        {
            if (request == null)
            {
                throw CreateException(HttpStatusCode.BadRequest, "Invalid or Incomplete Order information", 101416);
            }
            try
            {
                string obj = JsonConvert.SerializeObject(request);
                request.Data.Locale = Thread.CurrentThread.CurrentCulture.Name;
                SetOrderMemberId(request.Data);
                var response = new OrderResponseViewModel();
                List<ValidationErrorViewModel> errors = null;
                var result = _iMobileOrderProvider.Save(request.Data, ref errors);
                if (result != null)
                {
                    response.Data = result;
                    response.ValidationErrors = null != errors && errors.Any() ? errors : null;
                    JObject json = JObject.Parse(obj);
                    MobileActivityLogProvider.ActivityLog(json, response, request.Data.CustomerId, true,
                        this.Request.RequestUri.ToString(),
                        this.Request.Headers.ToString(),
                        this.Request.Headers.UserAgent.ToString(),
                        request.Data.Locale);

                    return response;
                }

                if (result != null)
                {
                    var orderResponseViewModel =  new OrderResponseViewModel {Data = request.Data};
                    JObject json1 = JObject.Parse(obj);
                    MobileActivityLogProvider.ActivityLog(json1, orderResponseViewModel, request.Data.CustomerId, true,
                        this.Request.RequestUri.ToString(),
                        this.Request.Headers.ToString(),
                        this.Request.Headers.UserAgent.ToString(),
                        request.Data.Locale);

                    return orderResponseViewModel;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
                throw CreateException(HttpStatusCode.InternalServerError,
                    "Internal server errror searching for Submit Order" + ex.Message, 404);
            }
            var orderResponseViewModel2 =  new OrderResponseViewModel {Data = null};

            MobileActivityLogProvider.ActivityLog(request, orderResponseViewModel2, request.Data.CustomerId, true,
                        this.Request.RequestUri.ToString(),
                        this.Request.Headers.ToString(),
                        this.Request.Headers.UserAgent.ToString(),
                        request.Data.Locale);
            
            return orderResponseViewModel2;
        }

        /// <summary>
        ///     Gets the order for the given orderId (guid)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="memberId"></param>
        /// <returns></returns>
        [HttpGet]
        [CamelCasingFilter]
        [UserTokenAuthenticate]
        public OrderResponseViewModel Get(Guid id, string memberId)
        {
            if (id == null)
            {
                throw CreateException(HttpStatusCode.BadRequest, "Invalid or Incomplete Order information", 101416);
            }
            try
            {
                var locale = Thread.CurrentThread.CurrentCulture.Name;
                var result = _iMobileOrderProvider.GetOrder(id, memberId, locale);
                if (result != null)
                {
                    var response =  new OrderResponseViewModel {Data = result};

                    MobileActivityLogProvider.ActivityLog(response, response, memberId, true,
                        this.Request.RequestUri.ToString(),
                        this.Request.Headers.ToString(),
                        this.Request.Headers.UserAgent.ToString(),
                        locale);

                    return response;

                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
                throw CreateException(HttpStatusCode.InternalServerError,
                    "Internal server errror searching for Get Orders" + ex.Message, 500);
            }
            var orderResponseViewModel3 =  new OrderResponseViewModel {Data = null};

            MobileActivityLogProvider.ActivityLog(string.Empty, orderResponseViewModel3, memberId, true,
                        this.Request.RequestUri.ToString(),
                        this.Request.Headers.ToString(),
                        this.Request.Headers.UserAgent.ToString(),
                        Thread.CurrentThread.CurrentCulture.Name);

            return orderResponseViewModel3;
        }

        /// <summary>
        ///     Gets the order for the given orderId (guid)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="memberId"></param>
        /// <returns></returns>
        [HttpDelete]
        [CamelCasingFilter]
        [UserTokenAuthenticate]
        public OrderResponseViewModel Delete(string id, string memberId, string paymentMethodName="")
        {
            if (string.IsNullOrEmpty(id) == null)
            {
                throw CreateException(HttpStatusCode.BadRequest, "Invalid or Incomplete Order information", 101416);
            }

            var locale = Thread.CurrentThread.CurrentCulture.Name;
            bool isApproved = false;
            paymentMethodName = locale == "zh-CN" && string.IsNullOrEmpty(paymentMethodName) ? "CN_99BillPaymentGateway" : paymentMethodName;
            var order = _iMobileOrderProvider.CancelOrder(memberId, id, locale, paymentMethodName, ref isApproved, Guid.Empty);
            if (null == order && isApproved)
            {
                throw CreateException(HttpStatusCode.NotAcceptable,
                    "order status approved", 111555);
            }
            return new OrderResponseViewModel {Data = order};
        }

        /// <summary>
        ///     Submits the order
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        [CamelCasingFilter]
        [SetCulture]
        [UserTokenAuthenticate]
        public OrderResponseViewModel Put(OrderRequestViewModel request)
        {
            if (request == null || request.Data.OrderItems == null)
            {
                throw CreateException(HttpStatusCode.BadRequest, "Invalid or Incomplete Order information", 101416);
            }
            string obj = JsonConvert.SerializeObject(request);
            SetOrderMemberId(request.Data);
            request.Data.Locale = Thread.CurrentThread.CurrentCulture.Name;

            //Logging the bare request before it gets modified as it is a reference type
            //MobileActivityLogProvider.ActivityLog(request, null, request.Data.MemberId, true,
            //            this.Request.RequestUri.ToString(),
            //            this.Request.Headers.ToString(),
            //            this.Request.Headers.UserAgent.ToString(),
            //            request.Data.Locale);


            var orderNumber = string.Empty;
            var isOrderSubmitted = false;
            if (_mobileQuoteHelper.CheckIfAnyOrderInProcessing(request.Data.MemberId, request.Data.Locale,
                ref orderNumber, ref isOrderSubmitted, request.Data.OrderNumber))
            {
                throw CreateException(HttpStatusCode.NotAcceptable,
                    "order still processing", 110406, orderNumber);
            }

            var authToken = Guid.Empty;
            if (null != Request && null != Request.Headers && Request.Headers.Any())
            {
                if (Request.Headers.Contains("X-HLUSER-TOKEN"))
                {
                    var authTokenValue = Request.Headers.GetValues("X-HLUSER-TOKEN").FirstOrDefault();
                    if (!string.IsNullOrEmpty(authTokenValue))
                    {
                        Guid.TryParse(authTokenValue, out authToken);
                    }
                }
            }

            var response = new OrderResponseViewModel();
            List<ValidationErrorViewModel> errors = null;
            var result = _iMobileOrderProvider.Submit(request.Data, ref errors, authToken, request.Data.MemberId);
            if (result != null)
            {
                response.Data = result;
                response.ValidationErrors = null != errors && errors.Any() ? errors : null;
                JObject json = JObject.Parse(obj);
                MobileActivityLogProvider.ActivityLog(json, response, request.Data.MemberId, true,
                        this.Request.RequestUri.ToString(),
                        this.Request.Headers.ToString(),
                        this.Request.Headers.UserAgent.ToString(),
                        request.Data.Locale);

                return response;
            }

            return new OrderResponseViewModel {Data = null};
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

        private HttpResponseException CreateException(HttpStatusCode httpStatusCode, string reasonText, int errorCode,
            string orderNumber)
        {
            var error = new HttpError(reasonText) {{"code", errorCode}};
            return new HttpResponseException(Request.CreateErrorResponse(httpStatusCode, error));
        }

        #endregion
    }
}