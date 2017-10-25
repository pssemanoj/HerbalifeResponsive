using MyHerbalife3.Shared.ViewModel.Models;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Web;
    using System.Web.Security;
    using HL.Common.Configuration;
    using HL.Common.Logging;
    using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
    using Providers;
    using MyHerbalife3.Ordering.Providers.Communication;
    using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

    public class HU_PayUPaymentGatewayResponse : PaymentGatewayResponse
    {

        #region Constants and Fields
        private const string CustomResponseBody = "<EPAYMENT>" + "{0}" + "|" + "{1}" + "</EPAYMENT>";
        private const string Order = "OrderNumber";
        private const string RefnoPayu = "REFNO";
        private const string StatusPayU = "ORDERSTATUS";
        private const string StatusAproveed = "COMPLETE";
        private const string StatusAuthorized = "PAYMENT_AUTHORIZED";
        private const string RefOrderNumber = "REFNOEXT";
        private const string IpnID = "IPN_PID[]";
        private const string IpnPname = "IPN_PNAME[]";
        private const string IpnDate = "IPN_DATE";

        private const string GateWay = "Agency";
        private const string AuthResult = "Status";
        private const string PaymentGateWayName = "PayU";
        private const string ResponseCode = "response_code";
        private const string AuthCode = "auth_code";
        private const string DigitalSecurity = "3Dsecure";
        private const string Rc = "RC";
        private const string Rt = "RT";
        private const string Ctrl = "ctrl";
        private const string DatePayU = "date";
        private const string Payrefno = "payrefno";

        private const string Redirect = "Yes";
        private const string CallBack = "Yes";
        private const string GateWayPost = "Redirect";
        private const string GateWayCallBack = "CallBack";
        private Dictionary<string, string> configEntries;

        #endregion

        public override bool CanProcess
        {
            get
            {
                bool canProcess = false;
                if ((QueryValues[GateWay] == PaymentGateWayName) && (QueryValues[GateWayPost] == Redirect))
                {
                    canProcess = true;
                    IsReturning = true;
                    OrderNumber = QueryValues[Order];
                    if (!string.IsNullOrEmpty(QueryValues[Payrefno]) && !string.IsNullOrEmpty(OrderNumber) && !string.IsNullOrEmpty(QueryValues[Rt]))
                    {
                        string key = GetConfigEntry("paymentGatewayKey");
                        key = Prehash(key);
                        string url = GetUrl();
                        string checkHash = GetMD5Hash(key, url);

                        if (checkHash != QueryValues[Ctrl])
                        {
                            LogSecurityWarning(PaymentGateWayName);
                            return canProcess;
                        }

                        if (QueryValues[Rt].StartsWith("000") || QueryValues[Rt].StartsWith("001"))
                        {
                            this.IsApproved = true;
                            AuthorizationCode = QueryValues[Payrefno];
                            TransactionCode = QueryValues[Payrefno];
                            this.CanSubmitIfApproved = false; // don't submit order when browser is redirected back. IPN will do it.

                            this.Status = OrderProvider.GetPaymentGatewayRecordStatus(OrderNumber);

                            // When browser is redirected back before IPN is post, use Defer Processing. This is the normal flow according to PayU.
                            // When browser is redirected back after IPN is post, order is already submitted, proceed like other gateways.
                            if (Status == PaymentGatewayRecordStatusType.Unknown)
                            {
                                this.IsPendingTransaction = true;

                                // log the response data here and then redirect to confirmation page.
                                PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Response, this.OrderNumber, string.Empty,
                                                                 "HU_PayUPaymentGateway", PaymentGatewayRecordStatusType.ApprovalPending, WebResponse);

                                // implement Defer Processing: redirect to confirmation page to show "Processing"
                                SessionInfo _sessionInfo = null;
                                string _locale = HLConfigManager.Configurations.Locale;
                                var member = ((MembershipUser<DistributorProfileModel>)Membership.GetUser()).Value;
                                string DistributorID = (null != member) ? member.Id : string.Empty;
                                if (!string.IsNullOrEmpty(DistributorID))
                                {
                                    _sessionInfo = SessionInfo.GetSessionInfo(DistributorID, _locale);
                                    if (_sessionInfo != null)
                                    {
                                        _sessionInfo.OrderStatus = SubmitOrderStatus.OrderSubmittedProcessing;
                                        _sessionInfo.OrderNumber = OrderNumber;
                                        try
                                        {
                                            var cmmSVCP= new CommunicationSvcProvider();
                                            cmmSVCP.SendEmailConfirmation(OrderNumber,"Processing");
                                        }
                                        catch (Exception ex)
                                        {
                                            LoggerHelper.Error(string.Format("Error sending email, order:{0} : {1}",OrderNumber,ex.Message));
                                            
                                        }
                                        HttpContext.Current.Response.Redirect("~/Ordering/Confirm.aspx?OrderNumber=" +
                                                                              _sessionInfo.OrderNumber);
                                        HttpContext.Current.Response.End();
                                    }
                                    else
                                    {
                                        LoggerHelper.Error(string.Format("HU_PayUPaymentGatewayResponse, Session is null. Order Number : {0} ", OrderNumber));
                                    }
                                }
                                else
                                {
                                    LoggerHelper.Error(string.Format("HU_PayUPaymentGatewayResponse, DistributorID is null. Order Number : {0} ",OrderNumber));
                                }
                            }
                        }
                    }
                    else
                    {
                        base.AuthResultMissing = true;
                    }

                }
                else if ((QueryValues[GateWay] == PaymentGateWayName) && (QueryValues[GateWayCallBack] == CallBack)) //This is a callback, = IPN
                {
                    canProcess = true;
                    if (string.IsNullOrEmpty(PostedValues[RefOrderNumber]))
                    {
                        LoggerHelper.Error("Missing Gateway REFNOEXT information ");
                        canProcess = false;
                    }
                    else
                    {
                        OrderNumber = PostedValues[RefOrderNumber];
                        AuthorizationCode = string.IsNullOrEmpty(PostedValues[RefnoPayu]) ? "" : PostedValues[RefnoPayu];
                        string statusSucess = string.IsNullOrEmpty(PostedValues[StatusPayU]) ? "" : PostedValues[StatusPayU];
                        if (string.IsNullOrEmpty(OrderNumber))
                        {
                            LogSecurityWarning(PaymentGateWayName);
                            return canProcess;
                        }
                        if (!string.IsNullOrEmpty(AuthorizationCode) && !string.IsNullOrEmpty(statusSucess))
                        {
                            if (statusSucess == StatusAproveed || statusSucess == StatusAuthorized)
                            {
                                IsApproved = true;
                                this.CanSubmitIfApproved = true;
                                this.SpecialResponse = BuildCustomResponseBody(CustomResponseBody);                      
                            }
                        }
                        else
                        {
                            base.AuthResultMissing = true;
                        }
                    }
                }
                return canProcess;

            }
        }

        protected override bool DetermineSubmitStatus()
        {
            bool canSubmit = false;
            if (IsReturning)
            {
                canSubmit = false; // don't submit when browser is redirected back; IPN do it.
            }
            else
            {
                    canSubmit = true;
                }
            return canSubmit;
        }

        public string GetUrl()
        {
            string Url = HttpContext.Current.Request.Url.AbsoluteUri;
            Url = HttpContext.Current.Request.Url.AbsoluteUri.Replace(":" + HttpContext.Current.Request.Url.Port.ToString(), "");
            string buildUrl = Url.Remove(Url.LastIndexOf("&"), (Url.Length - Url.LastIndexOf("&")));
            buildUrl = buildUrl.Contains("https") ? buildUrl : buildUrl.Replace("http", "https");
            buildUrl = buildUrl.Length.ToString() + buildUrl;
            return buildUrl;
        }

        private string BuildCustomResponseBody(string CustomResponseBody)
        {
            double gmtHungary = 9;
            string key = GetConfigEntry("paymentGatewayKey");
            key = Prehash(key);
            DateTime dt = DateTime.Now.AddHours(gmtHungary);
            string sendDate = dt.ToString("yyyyMMddHHmmss");
            string responseBody = string.Empty;
            string resposeToAgency = string.Empty;
            responseBody = PostedValues[IpnID].Length.ToString() + PostedValues[IpnID] + PostedValues[IpnPname].Length.ToString() + PostedValues[IpnPname] + PostedValues[IpnDate].Length.ToString() + PostedValues[IpnDate] + sendDate.Length.ToString() + sendDate;
            string Hash = GetMD5Hash(key, responseBody);
            resposeToAgency = string.Format(CustomResponseBody, sendDate, Hash);
            return resposeToAgency;
        }

        private string GetConfigEntry(string entryName)
        {
            this.configEntries = new Dictionary<string, string>();
            string entries = Settings.GetRequiredAppSetting("HU_PayUPaymentGateway");
            if (!string.IsNullOrEmpty(entries))
            {
                string[] allEntries = entries.Split(new[] { ';' });
                if (allEntries.Length > 0)
                {
                    foreach (string entry in allEntries)
                    {
                        string[] item = entry.Split(new[] { '=' });
                        if (item.Length > 1)
                        {
                            this.configEntries.Add(item[0], item[1]);
                        }
                    }
                }
            }

            string entryVal = string.Empty;
            if (!string.IsNullOrEmpty(entryName))
            {
                try
                {
                    entryVal = this.configEntries[entryName];
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
                            System.Threading.Thread.CurrentThread.CurrentCulture.Name,
                            entryName,
                            ex.Message);
                    LoggerHelper.Error(error);
                    throw;
                }
            }

            return entryVal;
        }

        public HU_PayUPaymentGatewayResponse()
        {
            base.GatewayName = this.GatewayName;
        }

        public string GetMD5Hash(string secretKey, string message)
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            var keyBytes = encoding.GetBytes(secretKey);
            var plaintextBytes = encoding.GetBytes(message);

            HMACMD5 md5 = new HMACMD5(keyBytes);
            var hash = md5.ComputeHash(plaintextBytes);

            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        //private string checkHMAC(string key, string message)
        //{
        //    if (key == null) return string.Empty;
        //    if (message == null) return string.Empty;
        //    string hmac1 = string.Empty;

        //    System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
        //    byte[] keyByte = encoding.GetBytes(key);
        //    HMACMD5 hmacmd5 = new HMACMD5(keyByte);
        //    byte[] messageBytes = encoding.GetBytes(message);
        //    byte[] hashmessage = hmacmd5.ComputeHash(messageBytes);
        //    hmac1 = ByteToString(hashmessage);

        //    return hmac1;
        //}

        private static string ByteToString(byte[] buff)
        {
            string sbinary = "";

            for (int i = 0; i < buff.Length; i++)
            {
                sbinary += buff[i].ToString("x2"); // hex format
            }
            return (sbinary);
        }

        private string Prehash(string key)
        {
            string hashKey = key.Replace("@@@@", "&T_=");
            return hashKey;
        }

    }
}



