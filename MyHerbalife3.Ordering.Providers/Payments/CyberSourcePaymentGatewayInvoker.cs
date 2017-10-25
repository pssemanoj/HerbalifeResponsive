using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    using System.Globalization;
    using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
    using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

    public class CyberSourcePaymentGatewayInvoker : PaymentGatewayInvoker
    {
        private const string TransactionType = "sale";

        private const string HKSecretKey = "HKSecretKey";
        private const string HKAccessKey = "HKAccessKey";
        private const string HKProfileId = "HKProfileId";

        private const string IDSecretKey = "IDSecretKey";
        private const string IDAccessKey = "IDAccessKey";
        private const string IDProfileId = "IDProfileId";

        private const string SGSecretKey = "SGSecretKey";
        private const string SGAccessKey = "SGAccessKey";
        private const string SGProfileId = "SGProfileId";

        private string AccessKey = string.Empty;
        private string ProfileId = string.Empty;
        private string SecretKey = string.Empty;

        private CyberSourcePaymentGatewayInvoker(string paymentMethod, decimal amount)
            : base("CyberSourcePaymentGateway", paymentMethod, amount)
        {
            GetCyberSourceKeyForLocale();
        }

        public override void Submit()
        {
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            IDictionary<string, string> unSignedParameters = new Dictionary<string, string>();
            string redirectUrl = _configHelper.GetConfigEntry("PaymentGatewayUrl");
            string accessKey = _configHelper.GetConfigEntry(AccessKey);
            string profileId = _configHelper.GetConfigEntry(ProfileId);

            string transactionGUID = Guid.NewGuid().ToString();
            var time = DateTime.Now.ToUniversalTime();
            string currentDateTime = time.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
            string locale = _locale;
            string referenceNumber = _orderNumber;
            string amount = _orderAmount.ToString();
            string currency = _currency;

            locale = locale == "en-SG" ? "en-US" : locale;

            MyHLShoppingCart myCart;
            SessionInfo sessionInfoMyCart = SessionInfo.GetSessionInfo(this._distributorId, this._locale);
            myCart = sessionInfoMyCart.ShoppingCart;
            if (myCart == null)
                myCart = ShoppingCartProvider.GetShoppingCart(this._distributorId, this._locale);

            var address = myCart.DeliveryInfo.Address.Address;
            string email = (null != myCart && !string.IsNullOrEmpty(myCart.EmailAddress)) ? myCart.EmailAddress.ToString(CultureInfo.InvariantCulture) : "null@cybersource.com";
            string postalCode = (null != address.PostalCode) ? address.PostalCode : "999999";
            HttpContext.Current.Response.Clear();

            var sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
            sb.AppendFormat("<form name='form' action='{0}' method='post'>", redirectUrl);
            sb.AppendFormat("<input type='hidden' name='access_key' value='{0}' >", accessKey);
            parameters.Add("access_key", accessKey);
            sb.AppendFormat("<input type='hidden' name='profile_id' value='{0}' >", profileId);
            parameters.Add("profile_id", profileId);
            sb.AppendFormat("<input type='hidden' name='transaction_uuid' value='{0}' >", transactionGUID);
            parameters.Add("transaction_uuid", transactionGUID);
            sb.AppendFormat("<input type='hidden' name='signed_date_time' value='{0}' >", currentDateTime);
            parameters.Add("signed_date_time", currentDateTime);
            sb.AppendFormat("<input type='hidden' name='locale' value='{0}' >", locale);
            parameters.Add("locale", locale);
            sb.AppendFormat("<input type='hidden' name='transaction_type' value='{0}' >", TransactionType);
            parameters.Add("transaction_type", TransactionType);
            sb.AppendFormat("<input type='hidden' name='reference_number' value='{0}' >", referenceNumber);
            parameters.Add("reference_number", referenceNumber);
            sb.AppendFormat("<input type='hidden' name='amount' value='{0}' >", amount);
            parameters.Add("amount", amount);
            sb.AppendFormat("<input type='hidden' name='currency' value='{0}' >", currency);
            parameters.Add("currency", currency);
            sb.AppendFormat("<input type='hidden' name='bill_to_address_line1' value='{0}' >", address.Line1);
            unSignedParameters.Add("bill_to_address_line1", address.Line1);
            sb.AppendFormat("<input type='hidden' name='bill_to_address_line2' value='{0}' >", address.Line2);
            unSignedParameters.Add("bill_to_address_line2", address.Line2);
            sb.AppendFormat("<input type='hidden' name='bill_to_address_city' value='{0}' >", address.City);
            unSignedParameters.Add("bill_to_address_city", address.City);
            sb.AppendFormat("<input type='hidden' name='bill_to_address_state' value='{0}' >",
                            address.StateProvinceTerritory);
            unSignedParameters.Add("bill_to_address_state", address.StateProvinceTerritory);
            sb.AppendFormat("<input type='hidden' name='bill_to_address_country' value='{0}' >", address.Country);
            unSignedParameters.Add("bill_to_address_country", address.Country);
            sb.AppendFormat("<input type='hidden' name='bill_to_address_postal_code' value='{0}' >", postalCode);
            unSignedParameters.Add("bill_to_address_postal_code", postalCode);
            sb.AppendFormat("<input type='hidden' name='bill_to_email' value='{0}' >", email);
            unSignedParameters.Add("bill_to_email", email);
            string signedFields = string.Join(",", parameters.Keys.ToArray());
            sb.AppendFormat("<input type='hidden' name='signed_field_names' value='{0}' >",
                            signedFields + ",signed_field_names");
            parameters.Add("signed_field_names", signedFields + ",signed_field_names");
            //string unSignedFields = string.Join(",", unSignedParameters.Keys.ToArray());
            //sb.AppendFormat("<input type='hidden' name='unsigned_field_names' value='{0}' >", unSignedFields + ",unsigned_field_names");
            sb.AppendFormat("<input type='hidden' id='signature' name='signature' value='{0}' >", sign(parameters));

            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");

            string response = sb.ToString();
            LogMessage(PaymentGatewayLogEntryType.Request, OrderNumber, _distributorId, _gatewayName,
                       PaymentGatewayRecordStatusType.Unknown, response);

            HttpContext.Current.Response.Write(response);
            HttpContext.Current.Response.End();
        }

        private string sign(IDictionary<string, string> paramsArray)
        {
            string secretKey = _configHelper.GetConfigEntry(SecretKey);
            return sign(buildDataToSign(paramsArray), secretKey);
        }

        private string sign(String data, String secretKey)
        {
            var encoding = new ASCIIEncoding();
            var keyByte = encoding.GetBytes(secretKey);

            var hmacsha256 = new HMACSHA256(keyByte);
            var messageBytes = encoding.GetBytes(data);
            return Convert.ToBase64String(hmacsha256.ComputeHash(messageBytes));
        }

        private string buildDataToSign(IDictionary<string, string> paramsArray)
        {
            var signedFieldNames = paramsArray["signed_field_names"].Split(',');
            IList<string> dataToSign = new List<string>();

            foreach (String signedFieldName in signedFieldNames)
            {
                dataToSign.Add(signedFieldName + "=" + paramsArray[signedFieldName]);
            }

            return commaSeparate(dataToSign);
        }

        private string commaSeparate(IList<string> dataToSign)
        {
            return String.Join(",", dataToSign.ToArray());
        }

        private void GetCyberSourceKeyForLocale()
        {
            switch (HLConfigManager.Configurations.Locale.ToUpper())
            {
                case "ID-ID":
                    SecretKey = IDSecretKey;
                    AccessKey = IDAccessKey;
                    ProfileId = IDProfileId;
                    break;
                case "ZH-HK":
                    SecretKey = HKSecretKey;
                    AccessKey = HKAccessKey;
                    ProfileId = HKProfileId;
                    break;
                case "EN-SG":
                    SecretKey = SGSecretKey;
                    AccessKey = SGAccessKey;
                    ProfileId = SGProfileId;
                    break;
            }
        }
    }
}