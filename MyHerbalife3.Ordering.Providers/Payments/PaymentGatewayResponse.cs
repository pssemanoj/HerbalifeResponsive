using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Web;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.Communication;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public abstract class PaymentGatewayResponse
    {
        #region Constants and Fields
        protected const string NoAuthResultInResponse = "{0} Response did not contain an Authorization result";
        protected const string NoOrderNumberInResponse = "{0} Response did not contain an Order Number";
        private const string FormDataPair = ";{0}={1}";
        private const string NoFormData = "No Data Present";
        private const string ResponseHadError = "The Response had errors: \r\n{0}\r\n";
        public static string PaymentGateWateSessionKey = "PAYMENT_GATEWAY_RESPONSE";
        public static string PGH_FPX_PaymentStatus = "PGH_FPX_PaymentStatus";

        public PaymentGatewayRecordStatusType Status = PaymentGatewayRecordStatusType.Unknown;
        private string errorMessage = string.Empty;
        #endregion

        #region Constructors and Destructors
        public PaymentGatewayResponse()
        {
            var filtered = new NameValueCollection(HttpContext.Current.Request.Form);
            if (filtered != null && filtered.HasKeys())
                filtered.Remove(null);
            PostedValues = filtered;

            filtered = new NameValueCollection(HttpContext.Current.Request.QueryString);
            if (filtered != null && filtered.HasKeys())
                filtered.Remove(null);
            QueryValues = filtered;

            //this.PostedValues = HttpContext.Current.Request.Form;
            //this.QueryValues = HttpContext.Current.Request.QueryString;
            TransactionCode = Guid.NewGuid().ToString();
            CardType = IssuerAssociationType.Visa;
        }
        #endregion

        #region Public Properties
        public abstract bool CanProcess { get; }
        public bool CanSubmitIfApproved { get; set; }
        public bool IsApproved { get; set; }
        public bool IsCancelled { get; set; }
        public bool IsPendingTransaction { get; set; }
        public bool IsReturning { get; set; }
        public string OrderNumber { get; set; }
        public bool ReloadShoppingCart { get; set; }
        public string SpecialResponse { get; set; }
        public string TransactionCode { get; set; }
        public SerializedOrderHolder PGHOrderHolder { get; set; }
        #endregion

        #region Properties
        protected bool AuthResultMissing { get; set; }
        protected string AuthorizationCode { get; set; }
        protected string CardNumber { get; set; }
        protected IssuerAssociationType CardType { get; set; }
        protected string GatewayName { get; set; }
        protected NameValueCollection PostedValues { get; set; }
        protected NameValueCollection QueryValues { get; set; }
        protected bool SuppressStatusLogging { get; set; }
        protected ConfigHelper _configHelper;

        protected virtual string WebResponse
        {
            get { return HttpContext.Current.Request.QueryString + "\r\n" + GetFormData(); }
        }
        #endregion

        #region Public Methods and Operators
        public static PaymentGatewayResponse Create()
        {
            return Create(true);
        }

        public static PaymentGatewayResponse Create(bool logTheResponse)
        {
            PaymentGatewayResponse response = null;
            if (HLConfigManager.Configurations.PaymentsConfiguration.HasPaymentGateway)
            {
                var handlers = GetPaymentGatewayResponseList();
                
                foreach (PaymentGatewayResponse handler in handlers)
                {
                    if (handler.CanProcess)
                    {
                        response = handler;
                        response.CanSubmitIfApproved = response.DetermineSubmitStatus();
                        if (handler.LogResponses(logTheResponse))
                        {
                            if (string.IsNullOrEmpty(response.errorMessage))
                            {
                                if (response.AuthResultMissing)
                                {
                                    response.errorMessage = string.Format(NoAuthResultInResponse, response.GatewayName);
                                }
                            }
                            if (string.IsNullOrEmpty(response.OrderNumber))
                            {
                                response.errorMessage = string.Format(NoOrderNumberInResponse, response.GatewayName);
                            }
                            string message = (!string.IsNullOrEmpty(response.errorMessage)) ? string.Format(ResponseHadError, response.errorMessage) : string.Empty;
                            if (!string.IsNullOrEmpty(response.errorMessage))
                            {
                                PaymentGatewayInvoker.LogBlindError(message);
                            }
                            else
                            {
                                var statusType = PaymentGatewayRecordStatusType.Unknown;
                                if (response.IsReturning)
                                {
                                    statusType = response.Status;
                                    if (statusType != PaymentGatewayRecordStatusType.OrderSubmitted)
                                    {
                                        statusType = (response.IsApproved) ? PaymentGatewayRecordStatusType.Approved : PaymentGatewayRecordStatusType.Declined;
                                        statusType = (response.IsPendingTransaction) ? PaymentGatewayRecordStatusType.ApprovalPending : statusType;
                                        statusType = (response.IsCancelled) ? PaymentGatewayRecordStatusType.CancelledByUser : statusType;
                                    }
                                }
                                else
                                {
                                    statusType = (response.IsApproved) ? PaymentGatewayRecordStatusType.Approved : PaymentGatewayRecordStatusType.Declined;
                                    statusType = (response.IsPendingTransaction) ? PaymentGatewayRecordStatusType.ApprovalPending : statusType;
                                    statusType = (response.IsCancelled) ? PaymentGatewayRecordStatusType.CancelledByUser : statusType;
                                    statusType = (response.Status == PaymentGatewayRecordStatusType.OrderSubmitted) ? response.Status : statusType;
                                }
                                PaymentGatewayInvoker.LogMessageWithInfo(PaymentGatewayLogEntryType.Response, response.OrderNumber, string.Empty, response.GetType().Name.Replace("Response", string.Empty), statusType, response.WebResponse);
                            }
                        }
                    }
                }
            }

            return response;
        }

        public virtual void GetPaymentInfo(SerializedOrderHolder holder)
        {
            var payment = holder.BTOrder.Payments[0];
            var orderPayment = (holder.Order as Order_V01).Payments[0] as CreditPayment_V01;

            if (!string.IsNullOrEmpty(CardNumber))
            {
                payment.AccountNumber = CardNumber;
            }
            else
            {
                payment.AccountNumber = PaymentInfoProvider.GetDummyCreditCardNumber(IssuerAssociationType.Visa);
            }

            payment.PaymentCode = HLConfigManager.CurrentPlatformConfigs[holder.Locale].PaymentsConfiguration.PaymentGatewayPayCode;
            if (string.IsNullOrEmpty(payment.PaymentCode))
            {
                payment.PaymentCode = CreditCard.CardTypeToHPSCardType(CardType);
            }
            orderPayment.Card.IssuerAssociation = CreditCard.GetCardType(payment.PaymentCode);

            if (!string.IsNullOrEmpty(AuthorizationCode))
            {
                payment.AuthNumber = AuthorizationCode;
            }
            orderPayment.AuthorizationCode = payment.AuthNumber;

            if (!string.IsNullOrEmpty(TransactionCode))
            {
                payment.TransactionCode = TransactionCode;
            }
            orderPayment.TransactionID = payment.TransactionCode;

            //They don't always return the full cardnumber, so if less than 15, use the base method which has a fixer in it
            if (payment.AccountNumber.Length < 15)
            {
                payment.AccountNumber = new string('*', 16 - payment.AccountNumber.Length) + payment.AccountNumber;
            }
            orderPayment.Card.AccountNumber = payment.AccountNumber;
        }

        public void SendDeclinedEmail()
        {
            try
            {
                var cmmSVCP = new CommunicationSvcProvider();
                cmmSVCP.SendEmailConfirmation(OrderNumber, "PaymentDeclined");
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("Error sending declined email, order:{0} : {1}", OrderNumber, ex.Message));

            }
        }

        public void SendConfirmationEmail()
        {
            try
            {
                var cmmSVCP = new CommunicationSvcProvider();
                cmmSVCP.SendEmailConfirmation(OrderNumber, "OrderSubmitted");
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("Error sending confirmation email, order:{0} : {1}", OrderNumber, ex.Message));

            }
        }
        #endregion

        #region Methods
        protected virtual bool LogResponses(bool currentState)
        {
            return currentState;
        }

        protected void LogResponseFromGateway(string gatewayName)
        {
            string webResponse = String.Join(";", (from string name in HttpContext.Current.Request.Params
                                                  let element = name where element != null
                                                  let value = HttpContext.Current.Request.Params[element] where value != null
                                                  select String.Concat(name, ":=", HttpUtility.UrlDecode(value))).ToArray());

            PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Response, OrderNumber, string.Empty, gatewayName, PaymentGatewayRecordStatusType.Unknown, webResponse);
        }

        protected virtual bool DetermineSubmitStatus()
        {
            return true;
        }

        protected string GetFormData()
        {
            string formData = string.Empty;
            foreach (String key in PostedValues.Keys)
            {
                if (key == null)
                {
                    LoggerHelper.Error(string.Format("Form Data key was Null  : {0} ", OrderNumber));
                    continue;
                }
                formData += string.Format(FormDataPair, key, PostedValues[key]);
            }

            if (string.IsNullOrEmpty(formData))
            {
                formData = NoFormData;
            }

            return formData;
        }

        protected void LogSecurityWarning(string paymentGateway)
        {
            PaymentGatewayInvoker.LogBlindError(paymentGateway, string.Format("Securtity Warning: Posted data did not match querystring:\r\nPosted:{0}\r\nQueryString: {1}", GetFormData(), HttpContext.Current.Request.QueryString));
        }

        /// <summary>
        ///     Have so many now, let's implement this cleaner.
        /// </summary>
        /// <returns>All the implemented Paymen Gateway Response Handlers</returns>
        private static List<PaymentGatewayResponse> GetPaymentGatewayResponseList()
        {
            return new List<PaymentGatewayResponse>
                {
                new PGHPaymentGatewayResponse(),
                    new RBSPaymentGatewayResponse(),
                    new HSBCPaymentGatewayResponse(),
                    new MyGatePaymentGatewayResponse(),
                    new DecidirPaymentGatewayResponse(),
                    new WebPayPaymentGatewayResponse(),
                    new PagoElectronicoPaymentGatewayResponse(),
                    new MultiMerchantVisaNetPaymentGatewayResponse(),
                    new PolCardPaymentGatewayResponse(),
                    new PagosOnlinePaymentGatewayResponse(),
                    new PuntoWebPaymentGatewayResponse(),
                    new ChronoPayPaymentGatewayResponse(),
                    new CredomaticPaymentGatewayResponse(),
                    new RomcardPaymentGatewayResponse(),
                    new PayclubPaymentGatewayResponse(),
                    new ProdubancoPaymentGatewayResponse(),
                    new PagoSeguroPaymentGatewayResponse(),
                    new AtcPaymentGatewayResponse(),
                    new CyberSourcePaymentGatewayResponse(),
                    new CL_ServiPagPaymentGatewayResponse(),
                    new UY_VpaymentPaymentGateWayResponse(),
                    new UY_OcaPaymentGateWayResponse(),
                    new PY_BanCardPaymentGateWayResponse(),
                    new CO_PsePaymentGatewayResponse(),
                new HU_PayUPaymentGatewayResponse(),
                new CN_99BillPaymentGatewayResponse(), 
                new CN_99BillCNPResponse(),
                new CN_99BillQuickPayResponse(),
                    new VN_VNPayPaymentGatewayResponse(),
                    new RS_BancaintesaPaymentGateWayResponse(),
                new MK_TutunskaPaymentGateWayResponse(),
                new PF_OsbPaymentGateWayResponse(),
                new BankSlipPaymentGatewayResponse(),
                new BradescoElectronicTransferPaymentGatewayResponse(),
                new BancodoBrazilElectronicTransferPaymentGatewayResponse(),
                new BankSlipBrasPagPaymentGatewayResponse(),
                new BankSlipBoldCronPaymentGatewayResponse(),
                new ItauBoldCronElectronicTransferPaymentGatewayResponse(),
                new ItauElectronicTransferPaymentGatewayResponse()
                };
        }
        #endregion

    }
}