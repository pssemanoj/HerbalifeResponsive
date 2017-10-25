using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class ChronoPayPaymentGatewayInvoker : PaymentGatewayInvoker
    {
        private ChronoPayPaymentGatewayInvoker(string paymentMethod, decimal amount)
            : base("ChronoPayPaymentGateway", paymentMethod, amount)
        {
        }

        public override void Submit()
        {
            string redirectUrl = _config.PaymentGatewayUrl;
            string successfulUrl = string.Concat(RootUrl, _config.PaymentGatewayReturnUrlApproved, "?Agency=ChronoPay&Status=Approved");
            string failedUrl = string.Concat(RootUrl, _config.PaymentGatewayReturnUrlDeclined, "?Agency=ChronoPay&Status=Declined");
            string callbackUrl = string.Concat(RootUrl, _config.PaymentGatewayReturnUrlApproved);
            string productId = _configHelper.GetConfigEntry("productId");
            string shareSec = _config.PaymentGatewayEncryptionKey;
            string orderNumber = _orderNumber.ToString();
            string price = _orderAmount.ToString().Replace(",", ".");
            string language = _config.PaymentGatewayStyle;
            string hash = GenerateHash(productId, price, shareSec);

            ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);
            // Post and redirect to ChronoPay website
            HttpContext.Current.Response.Clear();

            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
            sb.AppendFormat("<form name='form' action='{0}' method='post'>", redirectUrl);
            sb.AppendFormat("<input type='hidden' name='product_id' value='{0}'>", productId);
            sb.AppendFormat("<input type='hidden' name='product_price' value='{0}'>", price);
            sb.AppendFormat("<input type='hidden' name='cs1' value='{0}'>", orderNumber);
            sb.AppendFormat("<input type='hidden' name='cs2' value='{0}'>", price);
            sb.AppendFormat("<input type='hidden' name='cs3' value='{0}'>", "ChronoPay");
            sb.AppendFormat("<input type='hidden' name='language' value='{0}'>", language);
            sb.AppendFormat("<input type='hidden' name='cb_type' value='{0}'>", "P");
            sb.AppendFormat("<input type='hidden' name='cb_url' value='{0}'>",  callbackUrl);
            sb.AppendFormat("<input type='hidden' name='success_url' value='{0}'>", successfulUrl);
            sb.AppendFormat("<input type='hidden' name='decline_url' value='{0}'>", failedUrl);
            sb.AppendFormat("<input type='hidden' name='sign' value='{0}'>", hash);
            //sb.AppendFormat("<input type='submit' value='Pay via ChronoPay'/>");
            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");

            string response = sb.ToString();
            PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, response);

            HttpContext.Current.Response.Write(response);
            HttpContext.Current.Response.End();
        }

        private string GenerateHash(string productId, string productPrice, string shareSec)
        {
            string md5HashData = string.Concat(productId, "-", productPrice, "-", shareSec);

            MD5 md5Hasher = MD5.Create();
            byte[] secureHashByte = md5Hasher.ComputeHash(Encoding.Default.GetBytes(md5HashData));
            StringBuilder secureHashString = new StringBuilder();
            for (int i = 0; i < secureHashByte.Length; i++)
            {
                secureHashString.Append(secureHashByte[i].ToString("x2"));
            }

            return secureHashString.ToString();
        }
    }
}
