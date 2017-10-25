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
    public class PagoSeguroPaymentGatewayInvoker : PaymentGatewayInvoker
    {
        private PagoSeguroPaymentGatewayInvoker(string paymentMethod, decimal amount) : base("PagoSeguroPaymentGateway", paymentMethod, amount)
        {

        }

        public override void Submit()
        {
            string redirectUrl = _configHelper.GetConfigEntry("paymentGatewayUrl");
            //  string returnUrl = string.Concat(RootUrl, _config.PaymentGatewayReturnUrlApproved, "?Agency=PagoSeguro");
            string merchantId = _configHelper.GetConfigEntry("paymentGatewayMerchantId"); //MerchantId 
            string orderNumber = this._orderNumber;


            string pagoSeguroRedirectUrl = string.Empty;
            pagoSeguroRedirectUrl = GenerateUrl(redirectUrl, merchantId, orderNumber);

            // Post and redirect to PagoSeguro website
            HttpContext.Current.Response.Clear();

            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
            sb.AppendFormat("<form name='frmSolicitudPago' action='{0}' method='post'>", pagoSeguroRedirectUrl);
            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");
            string response = sb.ToString();
            PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, response);

            HttpContext.Current.Response.Write(response);
            HttpContext.Current.Response.End();
        }


        private string GenerateUrl(string redirectUrl, string merchantId, string orderNumber)
        {
            StringBuilder url = new StringBuilder();
            url.AppendFormat("{0}cID/{1}/RefID/{2}/Default.aspx", redirectUrl, merchantId, orderNumber);
            return url.ToString();
        }
    }
}
