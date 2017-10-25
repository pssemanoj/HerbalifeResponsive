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
    public class UY_OcaPaymentGateWayInvoker : PaymentGatewayInvoker
    {

        #region Constructors and Destructors

        private UY_OcaPaymentGateWayInvoker(string paymentMethod, decimal amount)
            : base("UY_OcaPaymentGateWay", paymentMethod, amount)
        {
        }

        #endregion

        #region Public Methods and Operators

        public override void Submit()
        {
            string redirectUrl = _configHelper.GetConfigEntry("paymentGatewayUrl");
            string redirectUrlProcessId = _configHelper.GetConfigEntry("paymentGatewayProcessIdUrl");
            string returnUrl = string.Concat(RootUrl, _config.PaymentGatewayReturnUrlApproved);
            string retuntMyHlUrl = string.Format("{0}?Agency=ocapayment&OrderId={1}", returnUrl, this.OrderNumber);
            string processId = string.Empty;
            string SaleRequestString = "?Nrocom={0}&Nroterm={1}&Moneda={2}&Importe={3}&Plan={4}&Tcompra={5}&Urlresponse={6}&Info={7}&Tconn={8}&ConsFinal={9}&Importegrbd={10}&Nrofactura={11}";
            string PresentacionRequestString = "<request><Idtrn>{0}</Idtrn><Nrocom>{1}</Nrocom><Nroterm>{2}</Nroterm><Moneda>{3}</Moneda><Importe>{4}</Importe><Plan>{5}</Plan><Info>{6}</Info><Urlresponse>{7}</Urlresponse><Tconn>{8}</Tconn></request>";
            string PresentacionSerializeRequestString = "A1={0}&A2={1}&A3={2}&A4={3}&A5={4}&A6={5}&A7={6}&A8={7}&A9={8}";
            var payment = HttpContext.Current.Session[PaymentInformation] as CreditPayment_V01;
            HttpContext.Current.Session.Remove(PaymentInformation);
            string installments = (null != payment)
                          ? (payment.PaymentOptions as PaymentOptions_V01).NumberOfInstallments.ToString()
                          : null;

            if (string.IsNullOrEmpty(installments))
            {
                throw new ApplicationException("No Credit Card Installment Value was found. Unable to proceed");
            }
            string nrocom;
            string nroterm;
            string orderNumber = this._orderNumber;
            string amount = (string.Format(this.getPriceFormat(this._orderAmount), this._orderAmount).Replace(".", "").Replace(",", ""));           
            SessionInfo myCart = SessionInfo.GetSessionInfo(this._distributorId, this._locale);
            OrderTotals_V01 totals = myCart.ShoppingCart.Totals as OrderTotals_V01;
            var hasDiscount = OrderProvider.HasVATDiscount(totals);
            string consFinal =  hasDiscount ? "1" : "0";           
            string importegrbd = hasDiscount ?
                (string.Format(
               this.getPriceFormat((null != totals ? totals.TaxableAmountTotal : 0)),
               (null != totals ? totals.TaxableAmountTotal : 0)).Replace(".", ""))
               .Replace(",", "") : string.Empty;
            string factura = hasDiscount ? orderNumber.Substring(3) : string.Empty;

            if (RootUrl.Contains("local") || RootUrl.Contains("qa") || RootUrl.Contains("uat") || RootUrl.Contains("10.36.158.182") || RootUrl.Contains("http://10.36.173.61"))
            {
                nrocom = _configHelper.GetConfigEntry("paymentGatewayNrocom");
                nroterm = _configHelper.GetConfigEntry("paymentGatewayNroterm");
            }
            else
            {
                // redirectUrl = GetConfigEntry("paymentGatewayBetaUrl");
                nrocom = _configHelper.GetConfigEntry("paymentGatewayBetaNrocom");
                nroterm = _configHelper.GetConfigEntry("paymentGatewayBetaNroterm");
            }

            System.Net.ServicePointManager.ServerCertificateValidationCallback = (ssender, certificate, chain, sslPolicyErrors) => true;
            string postData = SetSaleTransactionData(SaleRequestString, nrocom, nroterm, "858", amount, installments,
             "0", retuntMyHlUrl, orderNumber, "0", consFinal,importegrbd,factura);

            LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, "before processid"); //remove
            LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, postData);
            processId = getOcaTransactionId(postData).Trim();

            LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, "after processid");//remove

            string postPresentationData = SetPresentacionTransactionData(PresentacionRequestString, processId, nrocom, nroterm, "858", amount, installments,
            orderNumber, retuntMyHlUrl, "0");

            LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, postPresentationData);


            string postPresentationSerializeData = SetPresentacionTransactionData(PresentacionSerializeRequestString, processId, nrocom, nroterm, "858", amount, installments,
            orderNumber, returnUrl, "0", true);

            LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, postPresentationSerializeData);

            // Post and redirect to Produbanco website
            HttpContext.Current.Response.Clear();

            var sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""frmSolicitudPago""].submit()'>");
            sb.AppendFormat("<form name='frmSolicitudPago' action='{0}' method='post'>", redirectUrl);
            sb.AppendFormat("<input type='hidden' name='Idtrn' value='{0}'>", processId);
            sb.AppendFormat("<input type='hidden' name='Info' value='{0}'>", postPresentationSerializeData);
            sb.AppendFormat("<input type='hidden' name='Urlresponse' value='{0}'>", retuntMyHlUrl);
            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");

            string response = sb.ToString();
            LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, response);

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
        private string SetSaleTransactionData(string formatString, string nrocom, string numterm, string moneda, string importe, string plan, string tcompra, string urlresponse, string info, string Tconn, string consFinal,string importegrbd,string factura)
        {
            string agencyRequest = string.Format(formatString, nrocom, numterm, moneda, importe, plan,
             tcompra, urlresponse, info, Tconn, consFinal, importegrbd, factura);

            return agencyRequest;
        }

        private string SetPresentacionTransactionData(string formatString, string idtrn, string nrocom, string numterm, string moneda, string importe, string plan, string info, string urlresponse, string Tconn)
        {
            string agencyRequest = string.Format(formatString, idtrn, nrocom, numterm, moneda, importe, plan, info, urlresponse, Tconn);

            return agencyRequest;
        }

        private string SetPresentacionTransactionData(string formatString, string idtrn, string nrocom, string numterm, string moneda, string importe, string plan, string info, string urlresponse, string Tconn, bool serialize)
        {
            string agencyRequest = string.Format(formatString, idtrn, nrocom, numterm, moneda, importe, plan, info, urlresponse, Tconn);

            return agencyRequest;
        }

        protected string getPriceFormat(decimal number)
        {
            return HLConfigManager.Configurations.PaymentsConfiguration.AllowDecimal ? "{0:N2}" : (number == (decimal)0.0 ? "{0:0}" : "{0:#,###}");
        }


        protected string getOcaTransactionId(string data)
        {

            string transactionIdRequest = OrderProvider.SendOcaTransactionIdServiceRequest(data);
            return transactionIdRequest;
        }


        #endregion

    }
}