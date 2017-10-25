using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using OrderProvider = MyHerbalife3.Ordering.Providers.China.OrderProvider;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public partial class OrderPaymentResult : System.Web.UI.Page
    {
        public int rtnOk = 0;
        protected bool RedirectOrder = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                var postedValues = new NameValueCollection(HttpContext.Current.Request.Form);
                if (postedValues != null && postedValues.HasKeys())
                    postedValues.Remove(null);

                var queryValues = new NameValueCollection(HttpContext.Current.Request.QueryString);
                if (queryValues != null && queryValues.HasKeys())
                    queryValues.Remove(null);

                var strPostedValue = string.Join(", ",
                                                 postedValues.AllKeys.Select(key => key + ": " + postedValues[key])
                                                             .ToArray());
                var strQueryValue = string.Join(", ",
                                                queryValues.AllKeys.Select(key => key + ": " + queryValues[key])
                                                           .ToArray());

                LoggerHelper.Info(string.Format("Request Form value from 99 Bill : {0}", strPostedValue));
                LoggerHelper.Info(string.Format("Request QueryString value from 99 Bill : {0}", strQueryValue));
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("OrderPaymentResult page: Request Form value / QueryString from 99 Bill has error: {0}", ex.Message));
            }
            //Notify gateway for redirect orders
            var orderNumber = Request["rtnOrderNumber"] ?? string.Empty;
            var externalTraceNo = Request["externalTraceNo"] ?? string.Empty;
            
            if (!string.IsNullOrEmpty(orderNumber))
            {
                var status = Providers.OrderProvider.GetPaymentGatewayRecordStatus(orderNumber);
                if (status == PaymentGatewayRecordStatusType.Approved ||
                    status == PaymentGatewayRecordStatusType.OrderSubmitted)
                {
                    RedirectOrder = true;
                    rtnOk = 1;
                    LoggerHelper.Info(string.Format(
                        "Response sent to 99 Bill orderNumber: {0}; Status: {1}  ", orderNumber,
                        status.ToString()));
                }
                else
                {
                    var useRealTimeReprocessFor99Bill = Settings.GetRequiredAppSetting("UseRealTimeReprocessFor99Bill", false);
                    if(!useRealTimeReprocessFor99Bill) return;

                    var bankName = Request["BankId"] ?? string.Empty;
                    //OrderStatus
                    //0 -> No record found
                    //1 -> Processing
                    //2 -> ready to processing (for second(or nth) time)
                    //3 -> OrderImported
                    var isProcessing = Providers.OrderProvider.GetPaymentGatewayNotification(orderNumber);
                    switch (isProcessing)
                    {
                        case 0:
                            Providers.OrderProvider.InsertPaymentGatewayNotification(orderNumber, bankName);
                            if (CheckPendingOrderStatus(orderNumber))
                            {
                                RedirectOrder = true;
                                rtnOk = 1;
                                LoggerHelper.Info(string.Format(
                                    "Response sent to 99 Bill orderNumber: {0}  and Submitted", orderNumber));
                                return;
                            }
                            Providers.OrderProvider.UpdatePaymentGatewayNotification(orderNumber, 2);
                            break;
                        case 2:
                            Providers.OrderProvider.UpdatePaymentGatewayNotification(orderNumber, 1);
                            if (CheckPendingOrderStatus(orderNumber))
                            {
                                RedirectOrder = true;
                                rtnOk = 1;
                                LoggerHelper.Info(string.Format(
                                    "Response sent to 99 Bill orderNumber: {0} and Submitted ", orderNumber));
                                return;
                            }
                            Providers.OrderProvider.UpdatePaymentGatewayNotification(orderNumber, 2);
                            break;
                        case 1:
                            LoggerHelper.Info(string.Format("orderNumber: {0} is being processed; multiple request", orderNumber));
                            break;
                        case 3:
                            LoggerHelper.Info(string.Format("orderNumber: {0}; already Imported", orderNumber));
                            break;
                        default:
                             LoggerHelper.Info(string.Format("orderNumber: {0}; not valid scenario", orderNumber));
                            break;
                    }
                }
                return;
            }
            LoggerHelper.Info(string.Format("OrderPaymentResult Page info: Redirect OrderNumber: {0}; Pay by Phone OrderNumber: {1};  ", orderNumber,
                                             externalTraceNo));

            # region "To accept orders for quick money transaction system returns results"

            // Received the results 0: Success 1: Failure 
            var processFlag = Request["processFlag"] ?? string.Empty;
            // Received transaction type 
            var txnType = Request["txnType"] ?? string.Empty;
            // Receives the transaction amount to two decimal places 
            var amt = Request["amt"] ?? string.Empty;
            // Received external tracking number for businesses to order number 
            
            // Received operator number 
            var terminalOperId = Request["terminalOperId"] ?? string.Empty;
            // Received authorization code 
            var authCode = Request["authCode"] ?? string.Empty;
            // Received system reference number, a unique number generated by the system fast money 
            var RRN = Request["RRN"] ?? string.Empty;
            // Received terminal ID 
            var terminalId = Request["terminalId"] ?? string.Empty;
            // Received transaction time format: yyyyMMdd HHmmss 
            var txnTime = Request["txnTime"] ?? string.Empty;
            // Received abbreviated card 
            var shortPAN = Request["shortPAN"] ?? string.Empty;
            // Received return code 00 transactions for trading success 
            var responseCode = Request["responseCode"] ?? string.Empty;
            // Return information received transaction 
            var responseMessage = Request["responseMessage"] ?? string.Empty;
            //Name card issuers received 
            var issuerIdView = Request["issuerIdView"] ?? string.Empty;
            // Received card type 
            var cardType = Request["cardType"] ?? string.Empty;
            // Received the card issuer 
            var issuerId = Request["issuerId"] ?? string.Empty;
            // Receive the signature string 
            var signature = Request["signature"] ?? string.Empty;

            #endregion

            var val = processFlag + txnType + amt + externalTraceNo + terminalOperId + authCode + RRN + txnTime +
                      shortPAN + responseCode + cardType + issuerId;
            if (CerRsaVerifySignature(val, signature, Server.MapPath("~/App_Data/PaymentsKey99Bill/mgw.cer")) || Settings.GetRequiredAppSetting("PayByPhoneQATesting", false))
            {
                decimal totalPayment = 0;
                if (decimal.TryParse(amt, out totalPayment) && responseCode.Trim() == "00" && txnType.Trim().ToUpper() == "PUR")
                {
                    OrderProvider.ProcessPBPPaymentService(string.Empty, externalTraceNo);
                    LoggerHelper.Info(string.Format("Pay By Phone OrderNumber: {0} Processed; amt: {3}, ResponseCode: {1}, txnType: {2}", externalTraceNo, responseCode, txnType, amt));
                }
                else
                {
                    LoggerHelper.Error(string.Format("Pay By Phone: Payment Amount/responseCode/txnType failed!. OrderNumber: {0}, ResponseCode: {1}, txnType: {2}", externalTraceNo, responseCode, txnType));
                }
                //Data mortem sign success, returns 0 notify quick money received messages
                Response.Write(0);
            }
            else
            {
                //Sign test failure data, the data in question. Return 0 fast money received notification message
                Response.Write(0);
                LoggerHelper.Error(string.Format("Pay By Phone: Accept transaction notification failure!. OrderNumber: {0}", externalTraceNo));
            }
        }

        /// <summary> 
        /// Data test execution method 
        /// </summary>
        /// <param name="originalString">original stitching string</param>
        /// <param name="signatureString"> received encrypted string</param>
        /// <param name="pubkeyPath">certificate path</param>
        /// <returns> </returns> 
        public static bool CerRsaVerifySignature(string originalString, string signatureString, string pubkeyPath)
        {
            var originalByte = System.Text.Encoding.UTF8.GetBytes(originalString);

            var signatureByte = Convert.FromBase64String(signatureString);
            var x509 = new X509Certificate2(pubkeyPath);
            var rsapub = (RSACryptoServiceProvider)x509.PublicKey.Key;
            rsapub.ImportCspBlob(rsapub.ExportCspBlob(false));
            var f = new RSAPKCS1SignatureDeformatter(rsapub);
            f.SetHashAlgorithm("SHA1");
            var sha = new SHA1CryptoServiceProvider();
            var hashData = sha.ComputeHash(originalByte);
            return f.VerifySignature(hashData, signatureByte);
        }

        private bool CheckPendingOrderStatus(string orderNum)
        {
            var isOrderSubmitted = false;
            try
            {
                string response;
                var isApproved = OrderProvider.AnalyzePaymentGatewayOrder(orderNum, out response);
                if (isApproved)
                {
                    if(Providers.OrderProvider.SubmitPaymentGatewayOrder(orderNum, response))
                    {
                        Providers.OrderProvider.UpdatePaymentGatewayNotification(orderNum, 3);
                        isOrderSubmitted = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("CheckPendingOrderStatus error {0}", ex));
            }
            return isOrderSubmitted;
        }

        
    }
}