using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class VN_VNPayPaymentGatewayResponse : PaymentGatewayResponse
    {
        private const string GateWay = "Agency";
        private const string PaymentGateWayName = "VNPay";
        private const string Redirect = "Redirect";
        private const string VNPayData = "data";
        private const string VNPayHash = "sign";
        private const string PostResponseMessage = "Herbalife Update OK";
        private Dictionary<string, string> configEntries;

        public override bool CanProcess
        {
            get
            {
                bool canProcess = false;
                if (QueryValues[GateWay] == PaymentGateWayName) 
                {
                    canProcess = true;
                    string vnpayHash = QueryValues[VNPayHash];
                    if (string.IsNullOrEmpty(vnpayHash))
                    {
                        vnpayHash = PostedValues[VNPayHash];
                    }

                    string vnpayData = QueryValues[VNPayData];                   
                    if (string.IsNullOrEmpty(vnpayData))
                    {
                        vnpayData = PostedValues[VNPayData];
                    }

                    if (!string.IsNullOrEmpty(vnpayData))
                    {
                        vnpayData = vnpayData.Replace("%7c", "|").Replace("+", " ");
                    }
                    else
                    {
                        base.AuthResultMissing = true;
                        return canProcess;
                    }

                    var key = GetConfigEntry("Key");
                    var calculateHash = GetVNPaySignature(vnpayData, key);

                    //Validate this is not a spoof
                    if (vnpayHash != calculateHash)
                    {
                        LogSecurityWarning(PaymentGateWayName);
                        return canProcess; // donot stop from here when doing testing
                    }
                         
                    if (QueryValues[Redirect] == "True") // this is browser redirect back
                    {
                        this.IsReturning = true;
                        string[] vnpResults = vnpayData.Split('|');
                        if (vnpResults.Length > 5) // supposed to get 9
                        {
                            OrderNumber = vnpResults[2];
                            if (string.IsNullOrEmpty(OrderNumber))
                            {
                                LogSecurityWarning(PaymentGateWayName);
                            }
                            else
                            {
                                CanSubmitIfApproved = false;
                                this.Status = OrderProvider.GetPaymentGatewayRecordStatus(OrderNumber);
                                IsApproved = vnpResults[0] == "00";                           
                            }
                        }
                    }
                    else // this is backend post
                    {
                        string[] vnpResults = vnpayData.Split('|');
                        if (vnpResults.Length > 5) // supposed to get 9
                        {
                            OrderNumber = vnpResults[2];
                            if (string.IsNullOrEmpty(OrderNumber))
                            {
                                LogSecurityWarning(PaymentGateWayName);
                            }
                            else
                            {
                                AuthorizationCode = vnpResults[5];
                                this.SpecialResponse = ConstructPostResponse(vnpResults[0]);
                                CanSubmitIfApproved = true;
                                this.Status = OrderProvider.GetPaymentGatewayRecordStatus(OrderNumber);
                                IsApproved = vnpResults[0] == "00";
                                PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Request, OrderNumber, string.Empty, this.GatewayName, 
                                    PaymentGatewayRecordStatusType.Unknown, string.Concat("Response to VNPay Backend Post: ", SpecialResponse));

                                // DongABank transaction, need to call VNPay service to send "DELIVERED"
                                if (vnpResults[0] == "00" && vnpResults[6].ToUpper() == "DONGABANK")
                                {
                                    var proxy = ServiceClientProvider.GetOrderServiceProxy();

                                    var request = new SendVNPayDongaBankConfirmationRequest_V01()
                                    {
                                        Amount = vnpResults[3],
                                        OrderNumer = this.OrderNumber,
                                        PaymentMethod = "DONGABANK",
                                    };

                                    // For now, just send this confirmation and not checking the resposne, nor change the flow to submit order
                                    var response = proxy.SendVNPayDongaBankConfirmation(new SendVNPayDongaBankConfirmationRequest1(request)).SendVNPayDongaBankConfirmationResult as SendVNPayDongaBankConfirmationResponse_V01;
                                    if (null != response)
                                    {
                                        PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Response, this.OrderNumber, string.Empty, this.GatewayName,
                                                    PaymentGatewayRecordStatusType.Unknown, 
                                                    "DongABank DELIEVERED Confirmation response: " + response.DongaBankConfirmationResult);

                                        //string[] strArr = response.DongaBankConfirmationResult.Split('|');
                                        //if (strArr[0] == "00")
                                        //{

                                        //}
                                    }
                                    else
                                    {
                                        LoggerHelper.Error("DongABank DELIEVERED confirmation response is null.");
                                    }
                                }
                            }
                        }
                    }                
                }
                
                return canProcess;
            }
        }

        public VN_VNPayPaymentGatewayResponse()
        {
            base.GatewayName = this.GatewayName;
        }

        protected override bool DetermineSubmitStatus()
        {
            //POST and Redirect are supposedly ALWAYS POST first, then client Redirect
            if (IsReturning)
            {
                //This is a Client Redirect
                return false;
            }
            else
            {
                //This is a Server Post
                return true;
            }
        }

        private string GetVNPaySignature(string inStr, string key)
        {
            string md5HashData = string.Concat(inStr, "|", key);

            HashAlgorithm algorithemType = default(HashAlgorithm);
            var encoder = new ASCIIEncoding();
            byte[] valueByteArr = encoder.GetBytes(md5HashData);
            byte[] hashArr = null;

            algorithemType = new MD5CryptoServiceProvider();
            hashArr = algorithemType.ComputeHash(valueByteArr);

            var sb = new StringBuilder();
            foreach (byte b in hashArr)
            {
                sb.AppendFormat("{0:X2}", b);
            }

            return sb.ToString();
        }

        private string ConstructPostResponse(string responseCode)
        {
            string strConcat = string.Concat(responseCode.Trim(), "|", PostResponseMessage, "|", GetConfigEntry("TerminalCode"), "|", OrderNumber, "|", DateTime.Now.AddHours(14).ToString("yyyyMMddHHmmss"));
            string signature = GetVNPaySignature(strConcat, GetConfigEntry("Key"));

            string result = string.Concat(strConcat, "|", signature);
            return result;
        }

        private string GetConfigEntry(string entryName)
        {
            this.configEntries = new Dictionary<string, string>();
            string entries = Settings.GetRequiredAppSetting("VN_VNPayPaymentGateway");
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
                        throw new ApplicationException(string.Format("The Configuration Parameter {0} was found in external config, but it had no value", entryName));
                    }
                }
                catch (Exception ex)
                {
                    string error = string.Format("Missing Gateway information in External Config for: {0}, parameter: {1} Error: {2}",
                            System.Threading.Thread.CurrentThread.CurrentCulture.Name, entryName, ex.Message);
                    LoggerHelper.Error(error);
                    throw;
                }
            }

            return entryVal;
        }
    }
}
