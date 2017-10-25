using System.Text;
using System.Web;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class MultiMerchantVisaNetPaymentGatewayInvoker : PaymentGatewayInvoker
    {
        private MultiMerchantVisaNetPaymentGatewayInvoker(string paymentMethod, decimal amount)
            : base("MultiMerchantVisaNetPaymentGateway", paymentMethod, amount)
        {

        }

        public override void Submit()
        {
            string redirectUrl = _configHelper.GetConfigEntry("paymentGatewayUrl");
            string returnUrl = string.Concat(RootUrl, _config.PaymentGatewayReturnUrlApproved);
            string storeId = _configHelper.GetConfigEntry("paymentGatewayApplicationId");
            string orderNumber = this._orderNumber.Substring(2);
            string amount = this._orderAmount.ToString().Replace(",", string.Empty);

            // Post and redirect to MultipMerchantVisaNet website
            HttpContext.Current.Response.Clear();

            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
            sb.AppendFormat("<form name='form' action='{0}' method='post'>", redirectUrl);
            sb.AppendFormat("<input type='hidden' name='CODTIENDA' value='{0}'>", storeId);
            sb.AppendFormat("<input type='hidden' name='NUMORDEN' value='{0}'>", orderNumber);
            sb.AppendFormat("<input type='hidden' name='MOUNT' value='{0}'>", amount);

            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");

            string response = sb.ToString();
            PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, response);

            HttpContext.Current.Response.Write(response);
            HttpContext.Current.Response.End();
        }
    }
}
