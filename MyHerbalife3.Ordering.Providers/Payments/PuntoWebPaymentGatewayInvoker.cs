using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System.Text;
using System.Web;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class PuntoWebPaymentGatewayInvoker : PaymentGatewayInvoker
    {
        private PuntoWebPaymentGatewayInvoker(string paymentMethod, decimal amount)
            : base("PuntoWebPaymentGateway", paymentMethod, amount)
        {

        }

        public override void Submit()
        {
            string redirectUrl = _configHelper.GetConfigEntry("paymentGatewayUrl");
            string orderNum = this.OrderNumber;
            string commercialCode = _configHelper.GetConfigEntry("paymentGatewayApplicationId");
            string amount = this._orderAmount.ToString().Replace(",", string.Empty);
            string currency = this._currency;
            string transactionDate = System.DateTime.Today.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);  
            string tradeTime = System.DateTime.Now.ToString("HHMMss", System.Globalization.CultureInfo.InvariantCulture);
            string timeStamp = System.DateTime.Now.ToString("yyyyMMddHHmmssffff"); 
            string countryCode = this._country == "PE" ? "PER" : this._country;
            string clientCode = "REG0001"; // Distributor ID ?
            string Key = _configHelper.GetConfigEntry("paymentGatewayEncryptionKey");

            string[] data = new string[10];

            data[0] = commercialCode;
            data[1] = orderNum;
            data[2] = amount;
            data[3] = currency;
            data[4] = transactionDate;
            data[5] = tradeTime;
            data[6] = timeStamp;
            data[7] = clientCode;
            data[8] = countryCode;
            data[9] = Key;

            string dataToConvert = string.Join("", data);

            string signature = PuntoWebSignature.GenerateSignature(dataToConvert, Key);

            HttpContext.Current.Response.Clear();

            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
            sb.AppendFormat("<form name='form' action='{0}' method='post'>", redirectUrl);
            sb.AppendFormat("<input type='hidden' name='I1' value='{0}'>", commercialCode);
            sb.AppendFormat("<input type='hidden' name='I2' value='{0}'>", orderNum);
            sb.AppendFormat("<input type='hidden' name='I3' value='{0}'>", amount);
            sb.AppendFormat("<input type='hidden' name='I4' value='{0}'>", currency);
            sb.AppendFormat("<input type='hidden' name='I5' value='{0}'>", transactionDate);
            sb.AppendFormat("<input type='hidden' name='I6' value='{0}'>", tradeTime);
            sb.AppendFormat("<input type='hidden' name='I7' value='{0}'>", timeStamp);
            sb.AppendFormat("<input type='hidden' name='I8' value='{0}'>", clientCode);
            sb.AppendFormat("<input type='hidden' name='I9' value='{0}'>", countryCode);
            sb.AppendFormat("<input type='hidden' name='I10' value='{0}'>", HttpContext.Current.Server.UrlEncode(signature));
                
            
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
