using System.Web;
using System.Text;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using HL.Common.Configuration;
using HL.Common.Logging;
using System.Threading;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class PF_OsbPaymentGateWayResponse : PaymentGatewayResponse
    {
        private const string Order = "OrderNumber";
        private const string GateWay = "Agency";
        private const string AuthResult = "vads_result";
        private const string PaymentGateWayName = "ObsPaymentGateway";
        private const string GateWayCallback = "OsbCallBack";
        private const string GateWayRedirect = "OsbRedirect";
        private const string ObsType = "ObsType";
        private const string ResponseCode = "response_code";
        private const string AuthCode = "vads_auth_number";
        private const string Signature = "signature";
        private const string CardBrand = "vads_card_brand";
        private const string VisibleCardNumber = "vads_card_number";
        private const string PaymentCertificate = "vads_payment_certificate";
        private const string OrderId = "vads_order_id";


        private Dictionary<string, string> _configEntries;

        public override bool CanProcess
        {
            get
            {
                bool canProcess = false;
                this.ReloadShoppingCart = true;
                if ((QueryValues[GateWay] == PaymentGateWayName) && (QueryValues[ObsType] == GateWayRedirect))
                {
                    canProcess = true;
                    OrderNumber = QueryValues[Order];
                    if (string.IsNullOrEmpty(OrderNumber))
                    {
                        LogSecurityWarning(PaymentGateWayName);
                        return canProcess;
                    }

                    this.IsReturning = true;
                    string resultMatchRedirect = GetResult();
                    if ((!string.IsNullOrEmpty(PostedValues[Signature]) && resultMatchRedirect == PostedValues[Signature]))
                    {
                        IsApproved = (PostedValues[AuthResult] == "00"); // means transaction approved
                        if (IsApproved)
                        {
                            AuthorizationCode = PostedValues[AuthCode];
                            TransactionCode = PostedValues[PaymentCertificate];
                            CardType = MapCardType(!string.IsNullOrEmpty(PostedValues[CardBrand]) ? PostedValues[CardBrand] : string.Empty);
                            CardNumber = PostedValues[VisibleCardNumber];
                        }
                        if ((PostedValues[AuthResult] == "05") || (PostedValues[AuthResult] == "96"))
                        {
                            this.IsReturning = false;
                        }
                        IsCancelled = (PostedValues[AuthResult] == "17");
                    }
                    return canProcess;
                }
                else
                {
                    if ((QueryValues[GateWay] == PaymentGateWayName) && (QueryValues[ObsType] == GateWayCallback)) //This is a callback
                    {
                        string resultMatch = string.Empty;
                        canProcess = true;
                        OrderNumber = PostedValues[OrderId];
                        if (string.IsNullOrEmpty(OrderNumber))
                        {
                            LogSecurityWarning("PF_OsbPaymentGateWay");
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(PostedValues[AuthCode]))
                            {
                                resultMatch = GetResult();
                                if ((!string.IsNullOrEmpty(PostedValues[Signature]) && resultMatch == PostedValues[Signature]))
                                {
                                    IsApproved = (PostedValues[AuthResult] == "00"); // means transaction approved
                                    IsCancelled = (PostedValues[AuthResult] == "17");
                                    AuthorizationCode = PostedValues[AuthCode];
                                    TransactionCode = PostedValues[PaymentCertificate];
                                    CardType = MapCardType(!string.IsNullOrEmpty(PostedValues[CardBrand]) ? PostedValues[CardBrand] : string.Empty);
                                    CardNumber = PostedValues[VisibleCardNumber];
                                }
                            }
                            else
                            {
                                base.AuthResultMissing = true;
                            }
                        }
                    }
                }

                return canProcess;
            }
        }

        public PF_OsbPaymentGateWayResponse()
        {
            base.GatewayName = this.GatewayName;
        }


        private string GetResult()
        {
            string cert = string.Empty;
            cert = GetConfigEntry("paymentGatewayCertificate");
            var sortedDict = new SortedDictionary<string, string>(StringComparer.Ordinal);
            NameValueCollection nvc = new NameValueCollection();
            foreach (string keyItem in PostedValues.AllKeys) // We need to change to post for testing in BETA
            {
                string value = PostedValues[keyItem];
                string key = keyItem.ToString();

                if (key.Contains("vads_"))
                {
                    sortedDict.Add(key, value);
                }

            }

            string resultToCompare = string.Empty;
            string signedResultToCompare = string.Empty;
            // retrieve values:
            foreach (KeyValuePair<string, string> kvp in sortedDict)
            {
                resultToCompare = resultToCompare + kvp.Value + "+";
            }

            resultToCompare = resultToCompare + cert;
            PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Response, OrderNumber, string.Empty, "PF_OsbPaymentGateWay", PaymentGatewayRecordStatusType.Unknown, String.Format("Unsigned..{0}", resultToCompare));
            signedResultToCompare = GetSHA1(resultToCompare);
            PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Response, OrderNumber, string.Empty, "PF_OsbPaymentGateWay", PaymentGatewayRecordStatusType.Unknown, String.Format("signed..{0}", signedResultToCompare));
            return signedResultToCompare;
        }

        private static string GetSHA1(string str)
        {
            SHA1 sha1 = SHA1Managed.Create();
            // UTF8Encoding encoding = new UTF8Encoding();
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            byte[] stream = null;
            StringBuilder sb = new StringBuilder();
            stream = sha1.ComputeHash(encoding.GetBytes(str));
            // string cas = Convert.ToBase64String(stream);

            for (int i = 0; i < stream.Length; i++) sb.AppendFormat("{0:x2}", stream[i]);
            return sb.ToString();
        }

        private string GetConfigEntry(string entryName)
        {
            _configEntries = new Dictionary<string, string>();
            string configEntries = Settings.GetRequiredAppSetting("PF_OsbPaymentGateWay");
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

        protected override bool DetermineSubmitStatus()
        {
            //POST and Redirect are waiting for the call back first then the redirect back
            Status = OrderProvider.GetPaymentGatewayRecordStatus(OrderNumber);
            bool canSubmit = false;
            if (IsReturning)
            {
                //This is a Client Redirect
                if (IsApproved)
                {
                    if (Status == PaymentGatewayRecordStatusType.Unknown) //Client is back first (before the POST) or the Post never came
                    {
                        canSubmit = true; //We'll take the transaction
                    }
                    else
                    {
                        canSubmit = false;
                    }
                }
            }
            else
            {
                //This is a Server Post
                canSubmit = IsApproved;
            }

            return canSubmit;
        }

        private IssuerAssociationType MapCardType(string gatewayCardType)
        {
            IssuerAssociationType cardType;
            switch (gatewayCardType)
            {
                case "VISA":
                    cardType = IssuerAssociationType.Visa;
                    break;
                case "VISA_ELECTRON":
                    cardType = IssuerAssociationType.Visa;
                    break;
                case "MASTERCARD":
                    cardType = IssuerAssociationType.MasterCard;
                    break;
                case "AMEX":
                    cardType = IssuerAssociationType.AmericanExpress;
                    break;
                case "MAESTRO":
                    cardType = IssuerAssociationType.Maestro;
                    break;
                case "CB":
                    cardType = IssuerAssociationType.PaymentGateway;
                    break;
                case "E-CARTEBLEUE":
                    cardType = IssuerAssociationType.PaymentGateway;
                    break;
                default:
                    cardType = IssuerAssociationType.PaymentGateway;
                    break;
            }
            return cardType;
        }
    }
}


