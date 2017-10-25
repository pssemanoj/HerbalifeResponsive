using System.ServiceModel;
using System.Web;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;


namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class VN_VNPayPaymentGatewayInvoker : PaymentGatewayInvoker
    {
        private VN_VNPayPaymentGatewayInvoker(string paymentMethod, decimal amount)
            : base("VN_VNPayPaymentGateway", paymentMethod, amount)
        {
        }

        public override void Submit()
        {
            var proxy = ServiceClientProvider.GetOrderServiceProxy();

            var request = new GetVNPayPaymentServiceRequest_V01()
            {
                Amount = _orderAmount.ToString(),
                CurrencyCode = _currency,
                OrderNumer = _orderNumber,
            };

            var response = proxy.GetVNPayServiceBankUrl(new GetVNPayServiceBankUrlRequest(request)).GetVNPayServiceBankUrlResult as GetVNPayPaymentServiceResponse_V01;
            if (null != response)
            {
                LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName,
                            PaymentGatewayRecordStatusType.Unknown, response.VNResult);

                string[] strArr = response.VNResult.Split('|');
                if (strArr.Length > 2 && strArr[0] == "00")
                {
                    string redirectUrl = strArr[2];
                    HttpContext.Current.Response.Redirect(redirectUrl, true);
                }
            }
            else
            {
                LoggerHelper.Error("Unable to connect to VNPay");
            }
        }

   }
}
