using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class MK_TutunskaPaymentGateWayResponse : PaymentGatewayResponse
    {
        private const string Order = "trackid";
        private const string HerbalifeOrderNumber = "OrderNumber";
        private const string GateWayNameRedirect = "Agency";
        private const string GateWay = "udf1";
        private const string PaymentGateWayName = "Bankart";
        private const string GetUdf4Patamerer = "BankartCallBack";
        private const string GateWayRedirect = "Redirect";
        private const string Redirect = "Yes";
        private const string Tranid = "tranid";        
        private const string AuthCode = "auth";
        private const string ResponseCode = "result";
        private const string ErrorAgency = "Error";
        private const string ErrorText = "ErrorText";
        private const string PaymentGatewayId = "paymentid";
        private const string GateWayRedirectError = "GateWayErrorRedirect";
        private const string ErorDetails = "ErorDetails";
        private Dictionary<string, string> _configEntries;


        public override bool CanProcess
        {
            get
            {
                bool canProcess = false;
                if ((QueryValues[GateWayNameRedirect] == PaymentGateWayName)) //This is a Redirect from the Bank
                {
                    canProcess = true;

                    if ((QueryValues[GateWayRedirect] == Redirect)) //This is a Sucessfull Redirect
                    {                        
                        if (string.IsNullOrEmpty(QueryValues[HerbalifeOrderNumber]))
                        {
                            base.AuthResultMissing = true;
                        }
                        else
                        {
                            OrderNumber = QueryValues[HerbalifeOrderNumber];
                            if (string.IsNullOrEmpty(OrderNumber))
                            {
                                LogSecurityWarning(PaymentGateWayName);
                                return canProcess;
                            }                            
                            ProcessRedirectBackSuccesfull();
                        }
                    }
                    else
                    {                      
                        if ((QueryValues[GateWayRedirectError] == Redirect)) //This is an Error Redirect
                        {

                             var DeclinedOrderNumber = HttpContext.Current.Session["declinedOrderNumberMK"];                             
                                 if (DeclinedOrderNumber != null)
                                 {
                                     OrderNumber = DeclinedOrderNumber.ToString();
                                 }                                                      
                           
                                if (string.IsNullOrEmpty(OrderNumber))
                                {
                                    LogSecurityWarning(PaymentGateWayName);
                                    return canProcess;
                                }                                
                                ProcessRedirectBackError();
                        }                    
                    }                    
                }
                else
                {
                    if (!string.IsNullOrEmpty(PostedValues[GateWay])) //This is a callback
                    {
                        if (PostedValues[GateWay] == GetUdf4Patamerer)  //This is a Sucessfull Callback 
                        {                            
                            ProcessCallBackSuccesfull();
                            canProcess = true;
                        }

                    }
                    else
                    {
                          if ((!string.IsNullOrEmpty(PostedValues[ErrorAgency]) && !string.IsNullOrEmpty(PostedValues[ErrorText])
                            && !string.IsNullOrEmpty(PostedValues[PaymentGatewayId]) && ValidateUrl()))
                        {
                            IsApproved = false;
                            canProcess = true;
                            sendErrorUrl();
                        }
                    }


                }

                return canProcess;
            }
        }
        

        protected string RootUrl
        {
            get
            {
                return string.Concat(HttpContext.Current.Request.Url.Scheme, "://",
                                     HttpContext.Current.Request.Url.DnsSafeHost);
            }
        }

        private string GetConfigEntry(string entryName)
        {
            _configEntries = new Dictionary<string, string>();
            string configEntries = Settings.GetRequiredAppSetting("MK_TutunskaPaymentGateWay");
            if (!string.IsNullOrEmpty(configEntries))
            {
                var allEntries = configEntries.Split(new[] { ';' });
                if (allEntries.Length > 0)
                {
                    foreach (string entry in allEntries)
                    {
                        var item = entry.Split(new[] { '=' });
                        if (item.Length > 1)
                        {
                            _configEntries.Add(item[0], item[1]);
                        }
                    }
                }
            }

            string entryVal = string.Empty;
            if (!string.IsNullOrEmpty(entryName))
            {
                try
                {
                    entryVal = _configEntries[entryName];
                    if (string.IsNullOrEmpty(entryVal))
                    {
                        throw new ApplicationException(
                            string.Format(
                                "The Configuration Parameter {0} was found in external config, but it had no value",
                                entryName));
                    }
                }
                catch (Exception ex)
                {
                    string error =
                        string.Format(
                            "Missing Gateway information in External Config for: {0}, parameter: {1} Error: {2}",
                            Thread.CurrentThread.CurrentCulture.Name, entryName, ex.Message);
                    LoggerHelper.Error(error);
                    throw;
                }
            }

            return entryVal;
        }


        private void sendUrl()
        {           
            string returnUrl;
            string returnUrlPrefix = "REDIRECT=";
            string buildreturnUrl = string.Concat(RootUrl, GetConfigEntry("paymentGatewayReturnUrlApproved"));
            returnUrl = buildreturnUrl.Contains("https") ? buildreturnUrl : buildreturnUrl.Replace("http", "https");
            string returnUrlApproved = returnUrlPrefix + (string.Format("{0}?Agency=Bankart&OrderNumber={1}&Redirect=Yes", returnUrl, OrderNumber));
            PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Response, OrderNumber, string.Empty, "MK_TutunskaPaymentGateway", PaymentGatewayRecordStatusType.Unknown, String.Format("Url redirect..{0}", returnUrlApproved));
            this.SpecialResponse = returnUrlApproved;
        }

        private void sendErrorUrl()
        {
            string errorDetails = "ECode:" + PostedValues[ErrorAgency] + "Paymentcode:" + PostedValues[PaymentGatewayId];
            string returnUrl;
            string returnUrlPrefix = "REDIRECT=";
            string buildreturnUrl = string.Concat(RootUrl, GetConfigEntry("paymentGatewayReturnUrlApproved"));
            returnUrl = buildreturnUrl.Contains("https") ? buildreturnUrl : buildreturnUrl.Replace("http", "https");
            string returnUrlApproved = returnUrlPrefix + (string.Format("{0}?Agency=Bankart&GateWayErrorRedirect=Yes&ErorDetails={1}&", returnUrl, errorDetails));            
            this.SpecialResponse = returnUrlApproved;
        }


        public bool ValidateUrl()
        {
            string Url = HttpContext.Current.Request.Url.AbsoluteUri;
            bool ValidUrl;
            if (Url.Contains("https://mk") || Url.Contains("http://mk"))
            {
                ValidUrl = true;
            }
            else
            {
                ValidUrl = false;
            }
            return ValidUrl;
        }


        private void ProcessCallBackError()
        {        
            this.IsReturning = false;
            this.IsApproved = false;
            this.CanSubmitIfApproved = false;   
        }

        private void ProcessRedirectBackError()
        {
            this.IsReturning = false;
            this.IsApproved = false;
            this.CanSubmitIfApproved = false; 
        }

        private void ProcessRedirectBackSuccesfull()
        {
            this.IsReturning = true;
            this.CanSubmitIfApproved = false; 
            this.Status = OrderProvider.GetPaymentGatewayRecordStatus(OrderNumber);
            this.IsApproved = (this.Status == PaymentGatewayRecordStatusType.Approved || this.Status == PaymentGatewayRecordStatusType.OrderSubmitted);
        }


        private void ProcessCallBackSuccesfull()
        {

            if (!string.IsNullOrEmpty(PostedValues[AuthCode]))
            {
                OrderNumber = PostedValues[Order];
                if (string.IsNullOrEmpty(OrderNumber))
                {
                    LogSecurityWarning("TutunskaBank");
                }
                else
                {
                    this.IsApproved = PostedValues[ResponseCode] == "CAPTURED"; // we need to decide if we are doing direct payment or 2 phases - APPROVED /CAPTURED -
                    if (this.IsApproved)
                    {
                        // string cardType = PostedValues[ResultCardType];
                        this.CanSubmitIfApproved = true;   
                        this.CardType = IssuerAssociationType.PaymentGateway; // SetCreditCardType(cardType);
                        this.AuthorizationCode = null != PostedValues[AuthCode] ? PostedValues[AuthCode] : string.Empty;
                        this.TransactionCode = null != PostedValues[Tranid] ? PostedValues[Tranid] : string.Empty;                        
                    }
                    sendUrl();
                }
            }
            else
            {
                base.AuthResultMissing = true;
            }
        }            
    
    
        public MK_TutunskaPaymentGateWayResponse()
        {
            base.GatewayName = PaymentGateWayName;
        }
    }
}
