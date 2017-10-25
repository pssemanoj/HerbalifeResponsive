namespace MyHerbalife3.Ordering.Providers.Payments
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Web;
    using System.Net;
    using System.Web.Script.Serialization;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Xml;
    using System.Xml.Xsl;
    using System.Xml.Linq;

    using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
    using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

    public class PY_BanCardPaymentGateWayInvoker : PaymentGatewayInvoker
    {
        #region Constructors and Destructors

        private PY_BanCardPaymentGateWayInvoker(string paymentMethod, decimal amount)
            : base("PY_BanCardPaymentGateWay", paymentMethod, amount)
        {
        }

        #endregion

        #region Public Methods and Operators

        public override void Submit()
        {

            // string redirectUrl = string.Concat(GetConfigEntry("paymentGatewayUrl"), "?process_id="); 
            string redirectUrl = _configHelper.GetConfigEntry("paymentGatewayUrl");
            string redirectUrlProcessId = _configHelper.GetConfigEntry("paymentGatewayProcessIdUrl");
            string returnUrlApproved = string.Concat(RootUrl, _configHelper.GetConfigEntry("paymentGatewayReturnUrlApproved"));
            string approvedUrl = string.Format("{0}?Agency=Bancard&OrderId={1}&rsp=rsprdtc", returnUrlApproved, this.OrderNumber);
            string returnUrlDeclined = string.Concat(RootUrl, _configHelper.GetConfigEntry("paymentGatewayReturnUrlCanceled"));
            string declinedUrl = string.Format("{0}?Agency=Bancard&OrderId={1}", returnUrlDeclined, this.OrderNumber);
            string publicKey = _configHelper.GetConfigEntry("public_key");
            string privateKey = _configHelper.GetConfigEntry("private_Key");
            string dataProcessId = string.Empty;
            string processId = string.Empty;
            string token = string.Empty;

            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            nfi.NumberGroupSeparator = "";
            string amount = this._orderAmount.ToString("0.00", nfi);

            token = GenetateMD5(privateKey, OrderNumber.Remove(0, 2).TrimStart('0'), amount);

            dataProcessId = PrepareProcessIdData(publicKey, token, OrderNumber.Remove(0, 2).TrimStart('0'), amount, approvedUrl, declinedUrl);

            processId = getProcessId(dataProcessId);

            // Post and redirect to WebPay website
            HttpContext.Current.Response.Clear();

            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
            sb.AppendFormat("<form name='form' action='{0}' method='get'>", redirectUrl);
            sb.AppendFormat("<input type='hidden' name='process_id' value='{0}'>", processId);


            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");

            string response = sb.ToString();
            PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, response);

            HttpContext.Current.Response.Write(response);
            HttpContext.Current.Response.End();



        }

        #endregion


        #region Methods


        /// <summary>
        /// Put the requisite data elements into the authorization request
        /// </summary>
        /// <param name="requestLineItem">The request line item.</param>
        /// <returns></returns>
        private string PrepareProcessIdData(string public_key, string token, string orderNumber, string amount, string urlapproved, string urldeclined)
        {
            StringBuilder json = new StringBuilder();
            json.Append("{ \"public_key\"");
            json.AppendFormat(":\"{0}\",", public_key);
            json.Append("\"operation\": { ");
            json.AppendFormat("\"token\": \"{0}\",", token);
            json.AppendFormat("\"shop_process_id\": \"{0}\",", orderNumber);
            json.AppendFormat("\"amount\": \"{0}\",", amount);
            json.Append("\"currency\": \"PYG\",");
            json.Append("\"additional_data\": \"\",");
            json.Append("\"description\": \"Herbalife Paraguay\",");
            json.AppendFormat("\"return_url\": \"{0}\",", urlapproved);
            json.AppendFormat("\"cancel_url\": \"{0}\"", urldeclined);
            json.Append("}}");


            PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, json.ToString());
            return json.ToString();
        }

        private string GenetateMD5(string private_key, string shop_process_id, string amount)
        {
            // Generate secure hash
            // Notice the variables of vpc_ must follow this particular order, as well as the Form hidden variables below
            string md5HashData = private_key + shop_process_id + amount + "PYG";

            MD5 md5Hasher = MD5.Create();
            byte[] secureHashByte = md5Hasher.ComputeHash(Encoding.Default.GetBytes(md5HashData));
            StringBuilder secureHashString = new StringBuilder();

            for (int i = 0; i < secureHashByte.Length; i++)
                secureHashString.Append(secureHashByte[i].ToString("x2"));

            return secureHashString.ToString();
        }


        protected string getProcessId(string data)
        {
            string transactionIdRequest = OrderProvider.SendBanCardServiceRequest(data);
            return transactionIdRequest;
        }

        #endregion
    }
}



public class BancardProcessObject
{
    public string status { get; set; }
    public string process_id { get; set; }
}