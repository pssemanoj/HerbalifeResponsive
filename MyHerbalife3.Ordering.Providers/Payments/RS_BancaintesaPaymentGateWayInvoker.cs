using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Xml.Linq;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class RS_BancaintesaPaymentGateWayInvoker : PaymentGatewayInvoker
    {

        #region Constructors and Destructors

        private RS_BancaintesaPaymentGateWayInvoker(string paymentMethod, decimal amount)
            : base("RS_BancaintesaPaymentGateWay", paymentMethod, amount)
        {
        }

        #endregion

        #region Public Methods and Operators

        public override void Submit()
        {
            string redirectUrl = _configHelper.GetConfigEntry("paymentGatewayUrl");
            string returnUrl = string.Concat(RootUrl, _config.PaymentGatewayReturnUrlApproved);
            string returnUrlApproved = (string.Format("{0}?HBLOrderNumber={1}", returnUrl, this.OrderNumber));
            string returnUrlDeclined = (string.Format("{0}?Agency=Bancaintesa&OrderNumber={1}&ErrorCallBack=Yes", returnUrl, this.OrderNumber));// string.Concat(RootUrl, _configHelper.GetConfigEntry("paymentGatewayReturnUrlCanceled"));
            string processId = string.Empty;
            string responseInfoRaw;
            string responseId;
            //string SaleRequestString = "?Nrocom={0}&Nroterm={1}&Moneda={2}&Importe={3}&Plan={4}&Tcompra={5}&Urlresponse={6}&Info={7}&Tconn={8}";
            string SaleRequestString = "id={0}&password={1}&action={2}&langid={3}&currencycode={4}&amt={5}&responseURL={6}&errorURL={7}&trackid={8}&udf1={9}&udf2={10}&udf3={11}&udf4={12}&udf5={13}";


            string paymentGatewayId;
            string paymentGatewayPassword;
            string orderNumber = this._orderNumber;
            string amount = this._orderAmount.ToString().Replace(",", ".");
            paymentGatewayId = _configHelper.GetConfigEntry("paymentGatewayId");
            paymentGatewayPassword = _configHelper.GetConfigEntry("paymentGatewayPassword");

            System.Net.ServicePointManager.ServerCertificateValidationCallback = (ssender, certificate, chain, sslPolicyErrors) => true;
            string postData = SetSaleTransactionData(SaleRequestString, paymentGatewayId, paymentGatewayPassword, "1", "SRB", "941", amount, returnUrlApproved,
             returnUrlDeclined, orderNumber, "", "BancaintesaCallBack", "", "", "");

            LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, postData);
            responseInfoRaw = getBancaintesaTransactionId(postData);
            responseId = getIdFromAgencyResponse(responseInfoRaw);
            redirectUrl = getUrlFromAgencyResponse(responseInfoRaw);

            // Post and redirect to Produbanco website
            HttpContext.Current.Response.Clear();

            var sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""frmSolicitudPago""].submit()'>");
            sb.AppendFormat("<form name='frmSolicitudPago' action='{0}' method='get'>", redirectUrl);
            sb.AppendFormat("<input type='hidden' name='PaymentID' value='{0}'>", responseId);
            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");

            string response = sb.ToString();
            LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, response);

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

        /// <summary>
        /// Put the requisite data elements into the authorization request
        /// </summary>
        /// <param name="requestLineItem">The request line item.</param>
        /// <returns></returns>
        private string SetSaleTransactionData(string formatString, string id, string password, string action, string langid, string currencycode, string amount, string responseURL
            , string errorURL, string trackid, string udf1, string udf2, string udf3, string udf4, string udf5)
        {

            string agencyRequest = string.Format(formatString, id, password, action, langid, currencycode, amount, responseURL
            , errorURL, trackid, udf1, udf2, udf3, udf4, udf5);

            return agencyRequest;
        }



        protected string getPriceFormat(decimal number)
        {
            return HLConfigManager.Configurations.PaymentsConfiguration.AllowDecimal ? "{0:N2}" : (number == (decimal)0.0 ? "{0:0}" : "{0:#,###}");
        }



        protected string getBancaintesaTransactionId(string data)
        {

            string transactionIdRequest = OrderProvider.SendBancaIntesaPaymentServiceRequest(data);
            return transactionIdRequest;
        }

        protected string getUrlFromAgencyResponse(string data)
        {
            // Location of the letter ":"
            int i = data.IndexOf(':');

            // Remainder of string starting at ':'.
            string d = data.Substring(i + 1);
            return d;
        }


        protected string getIdFromAgencyResponse(string data)
        {
            // Location of the letter ":"
            int i = data.IndexOf(':');

            // Remainder of string starting at ':'.
            string d = data.Remove(i);
            return d;

        }




        #endregion

    }
}