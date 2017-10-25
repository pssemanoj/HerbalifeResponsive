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
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Mobile;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.WebAPI.Attributes;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyHerbalife3.Shared.Infrastructure.ValueObjects.ConfigurationManagement;

#endregion

namespace MyHerbalife3.Ordering.Controllers.Mobile
{
    /// <summary>
    ///     Announcement controller
    /// </summary>
    [CustomResponseHeader]
    public class MobileWechatController : ApiController
    {
        internal MobileQuoteHelper _mobileQuoteHelper;
        internal IMobileOrderProvider _mobileOrderProvider;
        internal IMobileWechatProvider _mobileWechatProvider;

        /// <summary>
        ///     ctor
        /// </summary>
        public MobileWechatController()
        {
            _mobileQuoteHelper = new MobileQuoteHelper();
            _mobileWechatProvider = new MobileWechatProvider(_mobileQuoteHelper);
            _mobileOrderProvider = new MobileOrderProvider(_mobileQuoteHelper, new MobileWechatHelper());
        }

        /// <summary>
        ///     Get announcement data.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [CamelCasingFilter]
        [SetCulture]
        [UserTokenAuthenticate]
        public MobileResponseWrapper Post(WechatPrepayIdRequestViewModel request, string memberId)
        {
            if (request == null || request.Data == null || request.Data.Prepay == null || request.Data.Order == null)
            {
                throw CreateException(HttpStatusCode.BadRequest, "request is null", 500404);
            }
            string obj = JsonConvert.SerializeObject(request);
            SetOrderMemberId(request.Data.Order);
            request.Data.Prepay.Locale = Thread.CurrentThread.CurrentCulture.Name;
            request.Data.Prepay.MemberId = memberId;
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

            WechatPrepayIdResponseViewModel result = null;
            var errors = new List<ValidationErrorViewModel>();
            if (null != request.Data.Order)
            {
                request.Data.Order.Locale = Thread.CurrentThread.CurrentCulture.Name;

                var amount = decimal.Zero;
                var id = _mobileWechatProvider.InsertToPaymentGatewayRecord(request.Data.Order,
                    authToken == Guid.Empty ? Guid.NewGuid() : authToken, ref amount);
                if (id == 0)
                {
                    errors.Add(new ValidationErrorViewModel
                    {
                        Code = 101416,
                        Reason = "InsertToPaymentGatewayRecord failed"
                    });
                }

                if (!string.IsNullOrEmpty(request.Data.Order.OrderNumber) && amount > 0)
                {
                    bool isLockedeach = true;
                    bool isLocked = true;
                    string lockfailed = string.Empty;


                    if (request.Data.Order.pcLearningPointOffSet > 0M && request.Data.Order.CategoryType != "ETO")
                    {
                        isLockedeach = OrderProvider.LockPCLearningPoint(request.Data.Prepay.MemberId, request.Data.Order.OrderNumber,
                                                          request.Data.Order.OrderMonth,
                                                          request.Data.Order.pcLearningPointOffSet, HLConfigManager.Platform);
                        if (!isLockedeach)
                        {
                            lockfailed = "PC Learning Point";
                            isLocked = false;
                        }
                    }
                    else if (request.Data.Order.pcLearningPointOffSet > 0M)
                    {
                        isLockedeach = OrderProvider.LockETOLearningPoint(
                                request.Data.Order.OrderItems.Select(s => s.Sku),
                                request.Data.Prepay.MemberId,
                                request.Data.Order.OrderNumber,
                                request.Data.Order.OrderMonth,
                                request.Data.Order.pcLearningPointOffSet,
                                HLConfigManager.Platform);

                        if (!isLockedeach)
                        {
                            lockfailed = "ETO Learning Point";
                            isLocked = false;
                        }
                    }
                    var shoppingcart = ShoppingCartProvider.GetShoppingCart(request.Data.Prepay.MemberId, request.Data.Prepay.Locale, true, false);
                    if (shoppingcart.HastakenSrPromotion)
                    {
                        isLockedeach = ChinaPromotionProvider.LockSRPromotion(shoppingcart, request.Data.Order.OrderNumber);
                        if (!isLockedeach)
                        {
                            lockfailed = lockfailed + ", SR Promotion";
                            isLocked = false;
                        }
                    }
                    if (shoppingcart.HastakenSrPromotionGrowing)
                    {
                        isLockedeach = ChinaPromotionProvider.LockSRQGrowingPromotion(shoppingcart, request.Data.Order.OrderNumber);
                        if (!isLockedeach)
                        {
                            lockfailed = lockfailed + ", SR Query Growing";
                            isLocked = false;
                        }
                    }
                    if (shoppingcart.HastakenSrPromotionExcelnt)
                    {
                        isLockedeach = ChinaPromotionProvider.LockSRQExcellentPromotion(shoppingcart, request.Data.Order.OrderNumber);
                        if (!isLockedeach)
                        {
                            lockfailed = lockfailed + ", SR Query Excellent";
                            isLocked = false;
                        }
                    }
                    if (shoppingcart.HastakenBadgePromotion)
                    {
                        isLockedeach = ChinaPromotionProvider.LockBadgePromotion(shoppingcart, request.Data.Order.OrderNumber);
                        if (!isLockedeach)
                        {
                            lockfailed = lockfailed + ", BadgePromotion";
                            isLocked = false;
                        }
                    }
                    if (shoppingcart.HastakenNewSrpromotion)
                    {
                        isLockedeach = ChinaPromotionProvider.LockNewSRPromotion(shoppingcart, request.Data.Order.OrderNumber);
                        if (!isLockedeach)
                        {
                            lockfailed = lockfailed + ", NewSrPromotion";
                            isLocked = false;
                        }
                    }

                    if (shoppingcart.HasBrochurePromotion)
                    {
                        isLockedeach = ChinaPromotionProvider.LockBrochurePromotion(shoppingcart, request.Data.Order.OrderNumber);
                        if (!isLockedeach)
                        {
                            lockfailed = lockfailed + ", BrochurePromotion";
                            isLocked = false;
                        }
                    }

                    if (isLocked)
                    {
                        result = _mobileWechatProvider.GetPrepayId(request.Data.Prepay, request.Data.Order.OrderNumber, amount);
                    }
                    else
                    {
                        errors.Add(new ValidationErrorViewModel
                        {
                            Code = 101417,
                            Reason = lockfailed.TrimStart(',') + " locking failed"
                        });
                    }

                }
            }

            var response = new MobileResponseWrapper
            {
                Data = result,
                ValidationErrors = errors.Any() ? errors : null
            };

            JObject json = JObject.Parse(obj);
            MobileActivityLogProvider.ActivityLog(json, response, memberId ?? string.Empty, true,
                Request.RequestUri.ToString(),
                Request.Headers.ToString(),
                Request.Headers.UserAgent.ToString(),
                request.Data.Prepay.Locale);

            return response;
        }

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


        [HttpGet]
        [CamelCasingFilter]
        [SetCulture]
        [UserTokenAuthenticate]
        public MobileResponseWrapper Get(string memberId, string orderNumber, string paymentMethodName, Guid id)
        {
            var response = new MobileResponseWrapper
            {
                Data = new WechatQueryOrderResponseViewModel { Status = false },
            };
            var paymentResponse = PaymentGatewayInvoker.CheckOrderStatus(paymentMethodName, orderNumber);
            if (paymentResponse != null && paymentResponse.Status == PaymentGatewayRecordStatusType.OrderSubmitted)
            {
                response = new MobileResponseWrapper
                {
                    Data = new WechatQueryOrderResponseViewModel {Status = true},
                };
                return response;
            }
            if (paymentResponse != null && paymentResponse.Status == PaymentGatewayRecordStatusType.Unknown)
            {
                var approved = false;
                _mobileOrderProvider.CancelOrder(memberId, orderNumber, Thread.CurrentThread.CurrentCulture.Name,paymentMethodName,
                    ref approved, id);
                response = new MobileResponseWrapper
                {
                    Data = new WechatQueryOrderResponseViewModel { Status = approved },
                };
                return response;
            }
            return response;
        }
        

        private HttpResponseException CreateException(HttpStatusCode httpStatusCode, string reasonText, int errorCode)
        {
            var error = new HttpError(reasonText) {{"code", errorCode}};
            return new HttpResponseException(Request.CreateErrorResponse(httpStatusCode, error));
        }
    }
}