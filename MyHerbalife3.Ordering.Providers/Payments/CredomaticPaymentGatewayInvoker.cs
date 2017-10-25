using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using HL.Common.Configuration;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class CredomaticPaymentGatewayInvoker : PaymentGatewayInvoker
    {

#region constants
        private string _creditCardNumber = string.Empty;
        private string _cvv = string.Empty;
        private string _expirationDate = string.Empty;
#endregion

        private CredomaticPaymentGatewayInvoker(string paymentMethod, decimal amount) : base("CredomaticPaymentGateway", paymentMethod, amount)
        {
        }

        public override void Submit()
        {
            string redirectUrl = _configHelper.GetConfigEntry("PaymentGatewayUrl"); // _config.PaymentGatewayUrl;
            string CardType = String.Empty;
            string TransactionType = "sale";
            var payment = HttpContext.Current.Session[PaymentInformation] as CreditPayment_V01;
            CardType = GetCardType(payment);
            string returnUrl = string.Concat(RootUrl, _config.PaymentGatewayReturnUrlApproved, (string.Format("?Agency=Credomatic&tc={0}", CardType)));

            string keyid = _config.PaymentGatewayApplicationId;
            string encryptionKey = _config.PaymentGatewayEncryptionKey;
            // string isTest = GetConfigEntry("PaymentGatewayMode") == "0" ? "1" : "0";
            string orderNumber = OrderNumber;
            string amount = _orderAmount.ToString();
            if (amount.Contains(","))
            {
                amount = amount.Replace(",", ".");
            }
            var ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            string epochTime = Convert.ToInt64(ts.TotalSeconds).ToString();

            string hash = GenerateHash(orderNumber, amount, epochTime, encryptionKey);

            // To disable Sale Transaction Type Only for Credomatic Agency
            if (HLConfigManager.Configurations.PaymentsConfiguration.DisableSale)
            {
                TransactionType = "auth";
            }

            // Post and redirect to Decidir website
            HttpContext.Current.Response.Clear();

            var sb = new StringBuilder();
            sb.Append("<html>");
            if (!Settings.GetRequiredAppSetting<bool>("TokenizationDisabled", false))
            {
                sb.AppendFormat(@"<body onload='document.getElementById(""ccnumber"").value = window.name; window.name = """"; document.forms[""form""].submit();'>");
            }
            else
            {
                sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
            }
            sb.AppendFormat("<form name='form' action='{0}' method='post'>", redirectUrl);
            sb.AppendFormat("<input type='hidden' name='type' value='{0}' size='8'>", TransactionType);
            sb.AppendFormat("<input type='hidden' name='key_id' value='{0}' size='10' >", keyid);
            sb.AppendFormat("<input type='hidden' name='hash' value='{0}' >", hash);
            sb.AppendFormat("<input type='hidden' name='time' value='{0}' size='12'>", epochTime);
            sb.AppendFormat("<input type='hidden' name='redirect' value='{0}'>", returnUrl);
            sb.AppendFormat("<input type='hidden' name='orderid' value='{0}'>", orderNumber);
            sb.AppendFormat("<input type='hidden' name='amount' value='{0}'>", amount);
            sb.AppendFormat("<input type='hidden' name='ccnumber' id='ccnumber' value='{0}'>", _creditCardNumber);
            sb.AppendFormat("<input type='hidden' name='ccexp' value='{0}'>", _expirationDate);
            sb.AppendFormat("<input type='hidden' name='cvv' value='{0}'>", _cvv);
            sb.AppendFormat("<input type='hidden' name='address' value='{0}'>", "");
            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");

            string response = sb.ToString();

            string logResponse = response.Replace(_creditCardNumber, payment.Card.AccountNumber).Replace(_cvv, payment.Card.CVV);
            LogMessage(PaymentGatewayLogEntryType.Request, OrderNumber, _distributorId, _gatewayName, PaymentGatewayRecordStatusType.Unknown, logResponse);

            HttpContext.Current.Response.Write(response);
            HttpContext.Current.Response.End();
        }

        private string GenerateHash(string orderId, string amount, string timeStamp, string encryptionKey)
        {
            string preHash = string.Concat(orderId, "|", amount, "|", timeStamp, "|", encryptionKey);
            var md5 = MD5.Create();
            var inputBytes = Encoding.UTF8.GetBytes(preHash);
            var hash = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2").ToLower());
            }
            return sb.ToString();
        }

        private string GetCardType(CreditPayment_V01 payment)
        {
            string theCardType = string.Empty;
            switch (payment.Card.IssuerAssociation)
            {
                case IssuerAssociationType.Visa:
                {
                    theCardType = "VI";
                    break;
                }
                case IssuerAssociationType.MasterCard:
                {
                    theCardType = "MC";
                    break;
                }
                case IssuerAssociationType.AmericanExpress:
                {
                    theCardType = "AX";
                    break;
                }
                default:
                {
                    theCardType = "VI";
                    break;
                }
            }
            return theCardType;
        }

        protected override void GetOrderNumber()
        {
            //Read off the card details for the agency, then mask the details for the logs
            var payment = HttpContext.Current.Session[PaymentInformation] as CreditPayment_V01;
            _creditCardNumber = payment.Card.AccountNumber;
            _expirationDate = payment.Card.Expiration.ToString("MMyy");
            _cvv = payment.Card.CVV;

            var cardNumber = _creditCardNumber.ToCharArray();
            for (int i = 1; i < cardNumber.Length - 4; i++)
            {
                cardNumber[i] = '*';
            }
            payment.Card.AccountNumber = new string(cardNumber);
            if (!string.IsNullOrEmpty(_cvv))
            {
                payment.Card.CVV = new string('*', _cvv.Length);
            }

            string orderData = _context.Session[PaymentGateWayOrder] as string;
            var holder = OrderSerializer.DeSerializeOrder(orderData);
            holder.BTOrder.Payments[0].AccountNumber = payment.Card.AccountNumber;
            holder.BTOrder.Payments[0].CVV = payment.Card.CVV;
            //holder.BTOrder.Payments[0].Expiration = _expirationDate;
            (holder.Order as Order_V01).Payments[0] = payment;
            _context.Session[PaymentGateWayOrder] = OrderSerializer.SerializeOrder(holder);

            base.GetOrderNumber();
        }
    }
}