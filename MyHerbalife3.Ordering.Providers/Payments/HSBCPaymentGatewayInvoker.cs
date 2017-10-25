using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Security.Cryptography;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;



namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class HSBCPaymentGatewayInvoker : PaymentGatewayInvoker
    {
        private HSBCPaymentGatewayInvoker(string paymentMethod, decimal amount)
            : base("HSBCPaymentGateway", paymentMethod, amount)
        {

        }

        public override void Submit()
        {
            //Dictionary<string, string> _configEntries = GetConfigEntries("HSBCPaymentGateway");
            string SECURE_SECRET = _configHelper.GetConfigEntry("PaymentGatewayEncryptionKey"); 
            string gatewayUrl = _config.PaymentGatewayUrl;
            string vpc_Merchant = _configHelper.GetConfigEntry("MerchantAccountName");
            string vpc_AccessCode = _configHelper.GetConfigEntry("PaymentGatewayPassword");
            string vpc_ReturnURL = string.Concat(RootUrl, _config.PaymentGatewayReturnUrlApproved);
            string vpc_Command = "pay"; 
            string vpc_Locale = "en";
            string vpc_Version = "1";

            string vpc_Amount = (_configHelper.GetConfigEntry("RoundTotals", false) == "true") ? (Convert.ToInt32(_orderAmount) * 100).ToString() : Convert.ToInt32((_orderAmount * 100)).ToString();
            string vpc_MerchTxnRef = "HSBC";
            string vpc_OrderInfo = _orderNumber;

            // Generate secure hash
            // Notice the variables of vpc_ must follow this particular order, as well as the Form hidden variables below
            string md5HashData = SECURE_SECRET + vpc_AccessCode + vpc_Amount + vpc_Command + vpc_Locale + vpc_MerchTxnRef + vpc_Merchant
                + vpc_OrderInfo + vpc_ReturnURL + vpc_Version;

            MD5 md5Hasher = MD5.Create();
            byte[] secureHashByte = md5Hasher.ComputeHash(Encoding.Default.GetBytes(md5HashData));
            StringBuilder secureHashString = new StringBuilder();
            for (int i = 0; i < secureHashByte.Length; i++)
            {
                secureHashString.Append(secureHashByte[i].ToString("x2"));
            }
            string secrueHash = secureHashString.ToString().ToUpper();

            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
            sb.AppendFormat("<form name='form' action='{0}' method='post'>", gatewayUrl);
            sb.AppendFormat("<input type='hidden' name='vpc_AccessCode' value='{0}'>", vpc_AccessCode);
            sb.AppendFormat("<input type='hidden' name='vpc_Amount' value='{0}'>", vpc_Amount);
            sb.AppendFormat("<input type='hidden' name='vpc_Command' value='{0}'>", vpc_Command);
            sb.AppendFormat("<input type='hidden' name='vpc_Locale' value='{0}'>", vpc_Locale);
            sb.AppendFormat("<input type='hidden' name='vpc_MerchTxnRef' value='{0}'>", vpc_MerchTxnRef);
            sb.AppendFormat("<input type='hidden' name='vpc_Merchant' value='{0}'>", vpc_Merchant);
            sb.AppendFormat("<input type='hidden' name='vpc_OrderInfo' value='{0}'>", vpc_OrderInfo);
            sb.AppendFormat("<input type='hidden' name='vpc_ReturnURL' value='{0}'>", vpc_ReturnURL);
            sb.AppendFormat("<input type='hidden' name='vpc_Version' value='{0}'>", vpc_Version);
            sb.AppendFormat("<input type='hidden' name='vpc_SecureHash' value='{0}'>", secrueHash);
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
