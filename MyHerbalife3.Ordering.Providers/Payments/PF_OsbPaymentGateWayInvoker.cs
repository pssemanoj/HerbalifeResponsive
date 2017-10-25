using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Text.RegularExpressions;
using HL.Common.Utilities;

using System.Security.Cryptography;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments

{
    public class PF_OsbPaymentGateWayInvoker : PaymentGatewayInvoker
    {

        private PF_OsbPaymentGateWayInvoker(string paymentMethod, decimal amount)
            : base("PF_OsbPaymentGateWay", paymentMethod, amount)
        {

        }

        public override void Submit()
        {
            string redirectUrl = _configHelper.GetConfigEntry("paymentGatewayUrl");
            string returnUrl = string.Concat(RootUrl, _configHelper.GetConfigEntry("paymentGatewayReturnUrlApproved"));
            string returnUrlDeclined = string.Concat(RootUrl, _configHelper.GetConfigEntry("paymentGatewayReturnUrlCanceled"));
            decimal amountDue = getAmountDue();
            string amount = (Math.Round(amountDue)).ToString().Replace(",", string.Empty).Replace(".", string.Empty);
            string returnUrlApproved = (string.Format("{0}?Agency=ObsPaymentGateway&OrderNumber={1}&ObsType=OsbRedirect", returnUrl, this.OrderNumber));
            string isTest = _configHelper.GetConfigEntry("PaymentGatewayMode") == "0" ? "TEST" : "PRODUCTION";

            DateTime thisTime = DateTime.Now;
            TimeZoneInfo tst = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            DateTime pfTime = TimeZoneInfo.ConvertTime(thisTime, TimeZoneInfo.Local, tst);
            string sendDate = pfTime.ToString("yyyyMMddHHmmss"); 

            String vads_action_mode = "INTERACTIVE";
            String vads_amount = amount;  //15.24  should be 1524 without decimal sing
            //String vads_available_languages = "";
            String vads_ctx_mode = isTest; 
            String vads_currency = "953";
            String vads_order_id = OrderNumber;
            String vads_page_action = "PAYMENT";
            String vads_payment_config = "SINGLE";
            String vads_return_mode = "POST";
            String vads_site_id = _configHelper.GetConfigEntry("paymentGatewaySiteId");  //"16635031";
            String vads_trans_date = sendDate;
            String vads_trans_id = this._orderNumber.Substring(4);
            String vads_url_cancel = returnUrlDeclined;
            String vads_url_return = returnUrlApproved; // "http://localhost:39137/WebSite1/osb.aspx";
            String vads_version = "V2";
            String certificate = _configHelper.GetConfigEntry("paymentGatewayCertificate"); // "9244314048249798";

            string cadenaa = BuildString(vads_action_mode,
          vads_amount
        , vads_ctx_mode
        , vads_currency
        ,vads_order_id 
        , vads_page_action
        , vads_payment_config
        , vads_return_mode
        , vads_site_id
        , vads_trans_date
        , vads_trans_id
        , vads_url_cancel
        , vads_url_return
        , vads_version
        , certificate);

            PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, cadenaa);
            string s = GetSHA1(cadenaa);
            // Post and redirect to MultipMerchantVisaNet website
            HttpContext.Current.Response.Clear();

            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
            sb.AppendFormat("<form name='form' action='{0}' method='post'>", redirectUrl);

            sb.AppendFormat("<input type=\"hidden\" name=\"vads_action_mode\" value=\"{0}\">\n", vads_action_mode);
            sb.AppendFormat("<input type=\"hidden\" name=\"vads_amount\" value=\"{0}\">\n", vads_amount);
            sb.AppendFormat("<input type=\"hidden\" name=\"vads_ctx_mode\" value=\"{0}\">\n", vads_ctx_mode);
            sb.AppendFormat("<input type=\"hidden\" name=\"vads_currency\" value=\"{0}\">\n", vads_currency);
            sb.AppendFormat("<input type=\"hidden\" name=\"vads_order_id\" value=\"{0}\">\n", vads_order_id);            
            sb.AppendFormat("<input type=\"hidden\" name=\"vads_page_action\" value=\"{0}\">\n", vads_page_action);
            sb.AppendFormat("<input type=\"hidden\" name=\"vads_payment_config\" value=\"{0}\">\n", vads_payment_config);
            sb.AppendFormat("<input type=\"hidden\" name=\"vads_return_mode\" value=\"{0}\">\n", vads_return_mode);
            sb.AppendFormat("<input type=\"hidden\" name=\"vads_site_id\" value=\"{0}\">\n", vads_site_id);
            sb.AppendFormat("<input type=\"hidden\" name=\"vads_trans_date\" value=\"{0}\">\n", sendDate);
            sb.AppendFormat("<input type=\"hidden\" name=\"vads_trans_id\" value=\"{0}\">\n", vads_trans_id);
            sb.AppendFormat("<input type=\"hidden\" name=\"vads_url_cancel\" value=\"{0}\">\n", vads_url_cancel);            
            sb.AppendFormat("<input type=\"hidden\" name=\"vads_url_return\" value=\"{0}\">\n", vads_url_return);
            sb.AppendFormat("<input type=\"hidden\" name=\"vads_version\" value=\"{0}\">\n", vads_version);

            sb.AppendFormat("<input type=\"hidden\" name=\"signature\" value=\"{0}\">\n", s);

            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");

            string response = sb.ToString();
            PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, response);

            HttpContext.Current.Response.Write(response);
            HttpContext.Current.Response.End();
        }


        public static string GetSHA1(string str)
        {
            SHA1 sha1 = SHA1Managed.Create();
            // UTF8Encoding encoding = new UTF8Encoding();
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            byte[] stream = null;
            StringBuilder sb = new StringBuilder();
            stream = sha1.ComputeHash(encoding.GetBytes(str));
            for (int i = 0; i < stream.Length; i++) sb.AppendFormat("{0:x2}", stream[i]);
            return sb.ToString();
        }

        public static string BuildString(
       string vads_action_mode,
       string vads_amount
       , string vads_ctx_mode
       , string vads_currency
        ,string vads_order_id 
       , string vads_page_action
       , string vads_payment_config
       , string vads_return_mode
       , string vads_site_id
       , string vads_trans_date
       , string vads_trans_id
       , string vads_url_cancel
       , string vads_url_return
       , string vads_version
       , string certificate)
        {

            return vads_action_mode
                + "+" + vads_amount
             + "+" + vads_ctx_mode
             + "+" + vads_currency
             +  "+" + vads_order_id 
             + "+" + vads_page_action
             + "+" + vads_payment_config
             + "+" + vads_return_mode
             + "+" + vads_site_id
             + "+" + vads_trans_date
             + "+" + vads_trans_id
             + "+" + vads_url_cancel 
             + "+" + vads_url_return
             + "+" + vads_version
             + "+" + certificate;
        }

        private decimal getAmountDue()
        {
            MyHLShoppingCart myCart;
            SessionInfo sessionInfoMyCart = SessionInfo.GetSessionInfo(this._distributorId, this._locale);
            myCart = sessionInfoMyCart.ShoppingCart;
            if (myCart == null)
                myCart = ShoppingCartProvider.GetShoppingCart(this._distributorId, this._locale);

            OrderTotals_V01 totals = myCart.Totals as OrderTotals_V01;
            return OrderProvider.GetConvertedAmount(totals.AmountDue, this._country);        
        }
    }
}
