using System;
using System.Text;
using System.Web;
using System.Security.Cryptography;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class RomcardPaymentGatewayInvoker : PaymentGatewayInvoker
    {
        private RomcardPaymentGatewayInvoker(string paymentMethod, decimal amount)
            : base("RomcardPaymentGateway", paymentMethod, amount)
        {

        }

        public override void Submit()
        {
            string redirectUrl = _configHelper.GetConfigEntry("paymentGatewayUrl");
            string returnUrl = string.Concat(RootUrl, _config.PaymentGatewayReturnUrlApproved);
            string terminal = _configHelper.GetConfigEntry("paymentGatewayTerminal");
            string merchantId = _config.PaymentGatewayApplicationId;
            string merchantName = _config.MerchantAccountName;
            string merchantUrl = "https://ro.myherbalife.com";
            string currency = "RON";

            string transactionType = "0"; // 0 for pre-auth, 21 for Sales Completion 
            string encryptionKey = _config.PaymentGatewayEncryptionKey;

            string gmt_offset = string.Empty;
            string country = string.Empty;
            string email = "testRomcard@herbalife.com";
            string timeStamp = System.DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmss");
            string nonce = GenerateNonce(32);

            string orderNum = _orderNumber.Substring(1, _orderNumber.Length-1); // Romcard only takes numeric for order #
            string amount = _orderAmount.ToString(".00").Replace(",", ".");

            // generate the mac and PSign 
            string mac = string.Concat(amount.Length.ToString(), amount, currency.Length.ToString(), currency, orderNum.Length.ToString(), orderNum, orderNum.Length.ToString(), orderNum, 
                merchantName.Length.ToString(), merchantName, merchantUrl.Length.ToString(), merchantUrl, merchantId.Length.ToString(), merchantId, terminal.Length.ToString(), 
                terminal, email.Length.ToString(), email, transactionType.Length.ToString(), transactionType, "--", timeStamp.Length.ToString(), timeStamp, 
                nonce.Length.ToString(), nonce, returnUrl.Length.ToString(), returnUrl);

            string pSign = Generate_PSign(encryptionKey, mac);

            // Post and redirect to Decidir website
            HttpContext.Current.Response.Clear();

            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
            sb.AppendFormat("<form name='form' action='{0}' method='post'>", redirectUrl);
            sb.AppendFormat("<input type='hidden' name='AMOUNT' value='{0}'>", amount);
            sb.AppendFormat("<input type='hidden' name='CURRENCY' value='{0}'>", currency);
            sb.AppendFormat("<input type='hidden' name='ORDER' value='{0}'>", orderNum);
            sb.AppendFormat("<input type='hidden' name='DESC' value='{0}'>", orderNum);
            sb.AppendFormat("<input type='hidden' name='MERCH_NAME' value='{0}'>", merchantName);
            sb.AppendFormat("<input type='hidden' name='MERCH_URL' value='{0}'>", merchantUrl);
            sb.AppendFormat("<input type='hidden' name='MERCHANT' value='{0}'>", merchantId);
            sb.AppendFormat("<input type='hidden' name='TERMINAL' value='{0}'>", terminal);
            sb.AppendFormat("<input type='hidden' name='EMAIL' value='{0}'>", email);
            sb.AppendFormat("<input type='hidden' name='TRTYPE' value='{0}'>", transactionType);
            sb.AppendFormat("<input type='hidden' name='COUNTRY' value='{0}'>", country);
            sb.AppendFormat("<input type='hidden' name='MERCH_GMT' value='{0}'>", gmt_offset);
            sb.AppendFormat("<input type='hidden' name='TIMESTAMP' value='{0}'>", timeStamp);
            sb.AppendFormat("<input type='hidden' name='NONCE' value='{0}'>", nonce);
            sb.AppendFormat("<input type='hidden' name='BACKREF' value='{0}'>", returnUrl);
            sb.AppendFormat("<input type='hidden' name='P_SIGN' value='{0}'>", pSign);
            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");


            string response = sb.ToString();
            PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, response);

            HttpContext.Current.Response.Write(response);
            HttpContext.Current.Response.End();
        }

        public static string Generate_PSign(string encryptionKey, string message)
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = PackH(encryptionKey);  //encoding.GetBytes(encryptionKey);
            HMACSHA1 hmacsha1 = new HMACSHA1(keyByte);

            byte[] messageByte = encoding.GetBytes(message);
            byte[] hashMessage = hmacsha1.ComputeHash(messageByte);

            string result = ByteToString(hashMessage);

            return result;
        }

        public static string ByteToString(byte[] buff)
        {
            string sbinary = "";
            for (int i = 0; i < buff.Length; i++)
            {
                sbinary += buff[i].ToString("X2"); // hex format
            }
            return (sbinary);
        }

        public static byte[] PackH(string hex)
        {
            if ((hex.Length % 2) == 1) hex += '0';
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        private string GenerateNonce(int length)
        {
            Random random = new Random();
            string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            StringBuilder result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                result.Append(characters[random.Next(characters.Length)]);
            }
            return result.ToString();
        }
    }
}
