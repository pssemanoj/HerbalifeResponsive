using System.ServiceModel;
using System.Text;
using System.Web;
using System.Threading;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class RBSPaymentGatewayInvoker : PaymentGatewayInvoker
    {
        private RBSPaymentGatewayInvoker(string paymentMethod, decimal amount)
            : base("RBSPaymentGateway", paymentMethod, amount)
        {
        }

        public override void Submit()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(_url);
            sb.AppendFormat("&country={0}", _country);
            sb.AppendFormat("&language={0}", Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName);
            sb.Append("&successURL=" + _context.Server.UrlEncode(string.Format("{0}{1}&Agency=RBS&OrderNumber={2}", RootUrl, _config.PaymentGatewayReturnUrlApproved, _orderNumber)));
            sb.AppendFormat("&failureURL=" + _context.Server.UrlEncode(string.Format("{0}{1}&Agency=RBS&OrderNumber={2}", RootUrl, _config.PaymentGatewayReturnUrlDeclined, _orderNumber)));
            sb.AppendFormat("&pendingURL=" + _context.Server.UrlEncode(string.Format("{0}{1}&Agency=RBS&OrderNumber={2}-{3}-Pending", RootUrl, _config.PaymentGatewayReturnUrlApproved, _orderNumber, _paymentMethod)));
            sb.Append("&" + _config.PaymentGatewayStyle);
            sb.AppendFormat("&preferredPaymentMethod={0}", (_paymentMethod == "CreditCard") ? string.Empty : _paymentMethod);

            string response = sb.ToString();
            LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, response);

            HttpContext.Current.Response.Redirect(response, true);
        }

        protected override void GetOrderNumber()
        {
            // find a few items stuck in HPS MSMQ that NL DS 28Y0005736 has a space at the end "28Y0005736 " so HPS thrown exception and won't call RBS for IDEAL URL
            _distributorId = _distributorId.Trim();

            var proxy = ServiceClientProvider.GetOrderServiceProxy();
            GenerateOrderNumberRequest_V01 request = new GenerateOrderNumberRequest_V01();
            request.Amount = _orderAmount;
            request.ClientName = ClientName;
            request.Country = _country;
            request.Currency = _currency;
            request.DistributorID = _distributorId;
            request.GenerateHPSID = true;
            request.PayCode = ("IDEAL-SSL.CreditCard".Contains(_paymentMethod)) ? "BW" : _paymentMethod;
            request.MerchantCode = _config.MerchantAccountName;
            GenerateOrderNumberResponse_V01 response =
                OrderProvider.GenerateOrderNumber(request) as GenerateOrderNumberResponse_V01;
            if (null != response)
            {
                _orderNumber = response.OrderID;
                _url = response.RedirectUrl;
                _orderNumber = response.OrderID;
                string orderData = _context.Session[PaymentGateWayOrder] as string;
                _context.Session.Remove(PaymentGateWayOrder);
                int recordId = OrderProvider.InsertPaymentGatewayRecord(_orderNumber, _distributorId, _gatewayName,
                                                                        orderData, _locale);
            }
        }
    }
}