using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System.Text;
using System.Web;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class AtcPaymentGateWayInvoker : PaymentGatewayInvoker
    {
        private AtcPaymentGateWayInvoker(string paymentMethod, decimal amount)
            : base("AtcPaymentGateway", paymentMethod, amount)
        {
        }

        public override void Submit()
        {
            string redirectUrl = _configHelper.GetConfigEntry("paymentGatewayUrl");
            //string returnUrlApproved = string.Concat(RootUrl, _config.PaymentGatewayReturnUrlApproved, "?Agency=ATC","&OrderNumber=",this._orderNumber);
            string returnUrlApproved =
                (string.Format("{0}?Agency=ATC-OrderNumber={1}",
                               string.Concat(RootUrl, _config.PaymentGatewayReturnUrlApproved), this.OrderNumber));
            string returnUrlDeclined =
                (string.Format("{0}?Agency=ATC-OrderNumber={1}",
                               string.Concat(RootUrl, _config.PaymentGatewayReturnUrlDeclined), this.OrderNumber));
            string atcComercio = _configHelper.GetConfigEntry("paymentGatewayCommerceId");
            string currencyAtcCode = _configHelper.GetConfigEntry("paymentGatewayCurrencyAtcCode");

            string orderNumber = this._orderNumber.Substring(2);
            string amount = this._orderAmount.ToString();
            string redirectUrlComplete = string.Empty;
            redirectUrlComplete = GenerateURL(redirectUrl, atcComercio, orderNumber, amount, currencyAtcCode,
                                              returnUrlApproved, returnUrlDeclined);
            // redirectUrlComplete = GenerateURL(redirectUrl, atcComercio, orderNumber, amount, currencyAtcCode, returnUrlApproved, returnUrlDeclined);

            // Post and redirect to ATC website
            HttpContext.Current.Response.Clear();

            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""frmSolicitudPago""].submit()'>");
            sb.AppendFormat("<form name='frmSolicitudPago' action='{0}' method='post'>", redirectUrlComplete);
            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");

            string response = sb.ToString();
            LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName,
                       PaymentGatewayRecordStatusType.Unknown, response);

            HttpContext.Current.Response.Write(response);
            HttpContext.Current.Response.End();
        }

        private string GenerateURL(string redirectUrl,
                                   string atcComercio,
                                   string orderNumber,
                                   string amount,
                                   string currencyAtcCode,
                                   string returnUrlApproved,
                                   string returnUrlDeclined)
        {
            string redirectUrlComplete = string.Empty;

            redirectUrlComplete = redirectUrl + "?ATCComercio=" + atcComercio + "&ATCINPUT=" + orderNumber + ";" +
                                  amount + ";" + currencyAtcCode +
                                  "&ATCOUTPUT=" + returnUrlApproved + "&ATCREC=" + returnUrlDeclined;

            return redirectUrlComplete;
        }
    }
}