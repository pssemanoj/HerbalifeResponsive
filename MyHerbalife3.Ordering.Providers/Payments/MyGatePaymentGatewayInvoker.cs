using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class MyGatePaymentGatewayInvoker : PaymentGatewayInvoker
    {
        private MyGatePaymentGatewayInvoker(string paymentMethod, decimal amount) : base("MyGatePaymentGateway", paymentMethod, amount)
        {

        }

        public override void Submit()
        {
            string redirectUrl = _config.PaymentGatewayUrl;
            string merchantId = _config.MerchantAccountName;
            string applicationId = _config.PaymentGatewayApplicationId;
            string mode = _configHelper.GetConfigEntry("PaymentGatewayMode");    // 0 means test, 1 means live
            string price = _orderAmount.ToString();
            string currency = _currency;
            string successfulUrl = _config.PaymentGatewayReturnUrlApproved;
            string failedUrl = _config.PaymentGatewayReturnUrlDeclined;

            // Post and redirect to HSBC website
            HttpContext.Current.Response.Clear();

            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
            sb.AppendFormat("<form name='form' action='{0}' method='post'>", redirectUrl);
            sb.AppendFormat("<input type='hidden' name='Mode' value='{0}'>", mode);
            sb.AppendFormat("<input type='hidden' name='txtMerchantID' value='{0}'>", merchantId);
            sb.AppendFormat("<input type='hidden' name='txtApplicationID' value='{0}'>", applicationId);
            sb.AppendFormat("<input type='hidden' name='txtMerchantReference' value='{0}'>", this.OrderNumber);
            sb.AppendFormat("<input type='hidden' name='txtPrice' value='{0}'>", price);
            sb.AppendFormat("<input type='hidden' name='txtCurrencyCode' value='{0}'>", currency);
            sb.AppendFormat("<input type='hidden' name='txtRedirectSuccessfulURL' value='{0}{1}'>", RootUrl, successfulUrl);
            sb.AppendFormat("<input type='hidden' name='txtRedirectFailedURL' value='{0}{1}'>", RootUrl, failedUrl);
            sb.AppendFormat("<input type='hidden' name='txtOrderNumber' value='{0}'>", this.OrderNumber);
            sb.AppendFormat("<input type='hidden' name='Variable1' value='{0}'>", this.OrderNumber);
            sb.Append("<input type='hidden' name='Variable2' value='MyGate'>");
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
