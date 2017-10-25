#region

using System;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.ViewModel.Model;
using System.Collections.Generic;
using System.Linq;
using HL.Common.Logging;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc;

#endregion

namespace MyHerbalife3.Ordering.Providers.Mobile
{
    public class MobileWechatProvider : IMobileWechatProvider
    {
        private MobileQuoteHelper _mobileQuoteHelper;
        public MobileWechatProvider(MobileQuoteHelper mobileQuoteHelper)
        {
            _mobileQuoteHelper = mobileQuoteHelper;
        }
        public WechatPrepayIdResponseViewModel GetPrepayId(WechatPrepayIdViewModel request, string orderNumber, decimal amount)
        {
            if (null == request)
            {
                return null;
            }

            if (string.IsNullOrEmpty(request.MemberId) || string.IsNullOrEmpty(request.Locale))
            {
                return null;
            }

            GetWechatPaymentPrepayIdResponse response = null;
            var totalFeeInCents = (int)(amount*100);
            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {
                response= proxy.GetWechatPaymentPrepayId(new GetWechatPaymentPrepayIdRequest1(new GetWechatPaymentPrepayIdRequest_V01
                {
                    Body =  request.Body,
                    Ip = request.Ip,
                    OrderNumber = orderNumber,
                    TotalFee = totalFeeInCents,
                })).GetWechatPaymentPrepayIdResult;
            }
            if (null != response)
            {
                var responseV01 = response as GetWechatPaymentPrepayIdResponse_V01;
                if (null != responseV01 && responseV01.Status == ServiceResponseStatusType.Success)
                {
                    return new WechatPrepayIdResponseViewModel
                    {
                        OrderNumber = orderNumber,
                        PrepayId = responseV01.PrepayId
                    };
                }
            }
            return null;
        }

        public int InsertToPaymentGatewayRecord(OrderViewModel orderViewModel, Guid authToken, ref decimal amount)
        {
            try
            {
                var errors = new List<ValidationErrorViewModel>();
                var myHlShoppingCart = _mobileQuoteHelper.PriceCart(ref orderViewModel, ref errors);
                if (null != errors && errors.Any())
                {
                    return 0;
                }
                var orderTotalsV01 = myHlShoppingCart.Totals as ServiceProvider.OrderSvc.OrderTotals_V01;
                var orderTotalsV02 = myHlShoppingCart.Totals as ServiceProvider.OrderSvc.OrderTotals_V02;

                var order = MobileOrderProvider.ModelConverter.ConvertOrderViewModelToOrderV01(orderViewModel,
                    ref errors, orderTotalsV02,
                    orderTotalsV01,
                    out amount);
                var countryCode = orderViewModel.Locale.Substring(3, 2);
                if (amount > 0)
                {
                    var orderNumber = GetOrderNumber(amount, countryCode, orderViewModel.OrderMemberId);
                    orderViewModel.OrderNumber = orderNumber;
                }
                else
                {
                    return 0;
                }

                var btOrder = OrderProvider.CreateOrder(order, myHlShoppingCart, countryCode, null, "Mobile");
                if (((ServiceProvider.SubmitOrderBTSvc.Order)btOrder).Payments != null)
                {
                    ((ServiceProvider.SubmitOrderBTSvc.Order)btOrder).OrderID = orderViewModel.OrderNumber;
                    ((ServiceProvider.SubmitOrderBTSvc.Order)btOrder).Payments[0].NumberOfInstallments = 1;
                    ((ServiceProvider.SubmitOrderBTSvc.Order)btOrder).Payments[0].NameOnAccount = "China User";
                    ((ServiceProvider.SubmitOrderBTSvc.Order)btOrder).Payments[0].Currency = "RMB";
                }
                var holder = OrderProvider.GetSerializedOrderHolder(btOrder, order, myHlShoppingCart,
                    authToken == Guid.Empty ? Guid.NewGuid() : authToken);
                var orderData = OrderSerializer.SerializeOrder(holder);

                return OrderProvider.InsertPaymentGatewayRecord(orderViewModel.OrderNumber, orderViewModel.MemberId,
                    "WechatPayment",
                    orderData, orderViewModel.Locale);
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("System.Exception", ex, "Exception in InsertToPaymentGatewayRecord method MobileWechatProvider");
                return 0;
            }
        }

        protected string GetOrderNumber(decimal amount, string country, string memberId)
        {
            var request = new ServiceProvider.OrderSvc.GenerateOrderNumberRequest_V01
            {
                Amount = amount,
                Country = country,
                DistributorID = memberId
            };
            var response = OrderProvider.GenerateOrderNumber(request);
            if (null != response)
            {
                return response.OrderID;
            }
            return string.Empty;
        }
    }
}