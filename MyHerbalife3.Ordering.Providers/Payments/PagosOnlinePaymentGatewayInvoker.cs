using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class PagosOnlinePaymentGatewayInvoker : PaymentGatewayInvoker
    {
        private PagosOnlinePaymentGatewayInvoker(string paymentMethod, decimal amount)
            : base("PagosOnlinePaymentGateway", paymentMethod, amount)
        {
        }

        public override void Submit()
        {
            string redirectUrl = _configHelper.GetConfigEntry("paymentGatewayUrl");
            //string returnUrl = string.Concat(RootUrl, _config.PaymentGatewayReturnUrlApproved);
            //string confirmationUrl = string.Concat(RootUrl, _config.PaymentGatewayReturnUrlApproved);
            string returnUrl = string.Concat(RootUrl, _configHelper.GetConfigEntry("paymentGatewayReturnUrlApproved"), "?Agency=PagosOnline");
            string confirmationUrl = string.Concat(RootUrl, _configHelper.GetConfigEntry("paymentGatewayReturnUrlApproved"), "?Agency=PagosOnline");
            string userId = _configHelper.GetConfigEntry("paymentGatewayApplicationId");
            string description = "Herbalife";
            string currency = this._currency.ToString();
            string isTest = _configHelper.GetConfigEntry("PaymentGatewayMode") == "0" ? "1" : "0";
            MyHLShoppingCart myCart;

            SessionInfo SessionInfoMyCart = SessionInfo.GetSessionInfo(this._distributorId, this._locale);
            myCart = SessionInfoMyCart.ShoppingCart;
            if (myCart == null)
                myCart = ShoppingCartProvider.GetShoppingCart(this._distributorId, this._locale);


            string email = (null != myCart && !string.IsNullOrEmpty(myCart.EmailAddress)) ? myCart.EmailAddress.ToString() : string.Empty;
            OrderTotals_V01 totals = myCart.Totals as OrderTotals_V01;
            decimal tax = (null != totals) ? totals.TaxAmount : 0;

            string orderNumber = this.OrderNumber;
            string amount = string.Format("{0:0.00}", this._orderAmount).Replace(",", ".");
            string refundbase = string.Format("{0:0.00}", this._orderAmount - tax).Replace(",", ".");
            string taxString = string.Format("{0:0.00}", tax).Replace(",", ".");
            string hash = generateSignature(userId, amount, orderNumber, currency);

            // Post and redirect to Pagos Online website
            HttpContext.Current.Response.Clear();

            var sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
            sb.AppendFormat("<form name='form' action='{0}' method='post'>", redirectUrl);
            sb.AppendFormat("<input type='hidden' name='usuarioId' value='{0}'>", userId);
            sb.AppendFormat("<input type='hidden' name='refVenta' value='{0}'>", orderNumber);
            sb.AppendFormat("<input type='hidden' name='descripcion' value='{0}'>", description);
            sb.AppendFormat("<input type='hidden' name='valor' value='{0}'>", amount);
            sb.AppendFormat("<input type='hidden' name='iva' value='{0}'>", taxString);
            sb.AppendFormat("<input type='hidden' name='moneda' value='{0}'>", currency);
            sb.AppendFormat("<input type='hidden' name='baseDevolucionIva' value='{0}'>", refundbase);
            sb.AppendFormat("<input type='hidden' name='emailComprador' value='{0}'>", email);
            sb.AppendFormat("<input type='hidden' name='firma' value='{0}'>", hash);
            sb.AppendFormat("<input type='hidden' name='prueba' value='{0}'>", isTest);
            sb.AppendFormat("<input type='hidden' name='url_respuesta' value='{0}'>", returnUrl);
            sb.AppendFormat("<input type='hidden' name='url_confirmacion' value='{0}'>", confirmationUrl);

            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");

            string response = sb.ToString();
            PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, response);

            HttpContext.Current.Response.Write(response);
            HttpContext.Current.Response.End();
        }

        private string generateSignature(string userId, string amount, string orderNumber, string currency)
        {
            //string encryptionKey = _config.PaymentGatewayEncryptionKey;
            string encryptionKey = _configHelper.GetConfigEntry("paymentGatewayEncryptionKey");

            var sb = new StringBuilder();
            sb.Append(encryptionKey);
            sb.Append("~");
            sb.Append(userId);
            sb.Append("~");
            sb.Append(orderNumber);
            sb.Append("~");
            sb.Append(amount);
            sb.Append("~");
            sb.Append(currency);

            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(sb.ToString()));

            sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("x2"));

            return sb.ToString();
        }
    }
}