using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;


namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class MK_TutunskaPaymentGateWayInvoker : PaymentGatewayInvoker
    {
        #region Constructors and Destructors

        private MK_TutunskaPaymentGateWayInvoker(string paymentMethod, decimal amount)
            : base("MK_TutunskaPaymentGateWay", paymentMethod, amount)
        {
        }

        #endregion

        #region Public Methods and Operators

        public override void Submit()
        {
            string redirectUrl = string.Empty;
            string responseId = string.Empty;
            string returnUrl = string.Concat(RootUrl, _configHelper.GetConfigEntry("paymentGatewayReturnUrlApproved"));
            string returnUrlDeclined = string.Concat(RootUrl, _configHelper.GetConfigEntry("paymentGatewayReturnUrlApproved"));

            string paymentGatewayAlias;
            
            string orderNumber = this._orderNumber;
            string amount = this._orderAmount.ToString().Replace(",", ".");

            paymentGatewayAlias = _configHelper.GetConfigEntry("paymentGatewayAlias");

            string redirectUrlList;
            redirectUrlList = GetRedirectUrl(returnUrl, returnUrlDeclined, paymentGatewayAlias, amount, orderNumber);
            
            responseId = getIdFromAgencyResponse(redirectUrlList);
            redirectUrl = getUrlFromAgencyResponse(redirectUrlList);
            redirectUrl = redirectUrl + "&?PaymentID=" + responseId;
 
            // Post and redirect to Tutunska website
            HttpContext.Current.Response.Clear();

            var sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
            sb.AppendFormat("<form name='form' action='{0}' method='post'>", redirectUrl);
           // sb.AppendFormat("<input type='hidden' name='PaymentID' value='{0}'>", responseId);

            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");
            HttpContext.Current.Session["declinedOrderNumberMK"] = OrderNumber;
            string response = sb.ToString();
            PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, response);

            HttpContext.Current.Response.Write(response);
            HttpContext.Current.Response.End();
        }

        public new string ResolveUrl(string originalUrl)
        {
            if (originalUrl != null && originalUrl.Trim() != "")
            {
                if (originalUrl.StartsWith("/"))
                    originalUrl = "~" + originalUrl;
                else
                    originalUrl = "~/" + originalUrl;

                originalUrl = HttpContext.Current.Server.MapPath(originalUrl);
            }

            if (originalUrl == null)
                return null;
            if (originalUrl.IndexOf("://", System.StringComparison.Ordinal) != -1)
                return originalUrl;
            if (originalUrl.StartsWith("~"))
            {
                string newUrl = "";
                if (HttpContext.Current != null)
                    newUrl = HttpContext.Current.Request.ApplicationPath + originalUrl.Substring(1).Replace("//", "/");
                return newUrl;
            }
            return originalUrl;
        }

        #endregion

        #region Methods



        private string GetRedirectUrl(string returnUrl, string returnUrlDeclined, string paymentGatewayAlias, string amount, string orderNumber)
        {
            string data;
            string buildreturnUrl;
            string buildreturnUrlDeclined;
            buildreturnUrl = returnUrl.Contains("https") ? returnUrl : returnUrl.Replace("http", "https");
            buildreturnUrlDeclined = returnUrlDeclined.Contains("https") ? returnUrlDeclined : returnUrlDeclined.Replace("http", "https");
            data = paymentGatewayAlias + "@" + amount + "@" + buildreturnUrl + "@" + buildreturnUrlDeclined + "@" + orderNumber;
            LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, data);
            string transactionIdRequest = OrderProvider.SendTutunskaPaymentServiceRequest(data);
            return transactionIdRequest;
        }

        protected string getUrlFromAgencyResponse(string data)
        {
            // Location of the letter ":"
            int i = data.LastIndexOf(':') - 1;

            // Remainder of string starting at ':'.
            string d = data.Remove(i, data.Length - i);
            return d;
        }


        protected string getIdFromAgencyResponse(string data)
        {
            // Location of the letter ":"
            int i = data.LastIndexOf(':');

            // Remainder of string starting at ':'.
            string d = data.Substring(i + 1);
            return d;

        }
        

        private Charge_V01 GetCharge(ChargeList chargeList, ChargeTypes type)
        {
            return chargeList.Find(delegate(Charge p) { return ((Charge_V01)p).ChargeType == type; }) as Charge_V01 ?? new Charge_V01(type, (decimal)0.0);
        }

        protected string getPriceFormat(decimal number)
        {
            return HLConfigManager.Configurations.PaymentsConfiguration.AllowDecimal ? "{0:N2}" : (number == (decimal)0.0 ? "{0:0}" : "{0:#,###}");
        }

        #endregion
    }
}