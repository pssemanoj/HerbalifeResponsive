using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;
using VPOS20_PLUGIN;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class ProdubancoPaymentGatewayResponse : PaymentGatewayResponse
    {
        private const string GateWay = "Agency";
        private const string PaymentGatewayName = "Produbanco";
        private const string sessionKey = "SESSIONKEY";
        private const string xmlRes = "XMLRES";
        private const string digitalSign = "DIGITALSIGN";

        protected CheckoutConfiguration _Check = HLConfigManager.Configurations.CheckoutConfiguration;
        protected PaymentsConfiguration _config = HLConfigManager.Configurations.PaymentsConfiguration;
        private Dictionary<string, string> _configEntries;

        public ProdubancoPaymentGatewayResponse()
        {
            base.GatewayName = GatewayName;
        }

        public override bool CanProcess
        {
            get
            {
                bool canProcess = false;
                if (QueryValues[GateWay] == PaymentGatewayName)
                {
                    canProcess = true;
                    //  OrderNumber = QueryValues[Order];
                    string sSESSIONKEY;
                    string sXMLRES;
                    string sDIGITALSIGN;
                    if (!string.IsNullOrEmpty(QueryValues[sessionKey]) && !string.IsNullOrEmpty(QueryValues[xmlRes]) &&
                        !string.IsNullOrEmpty(QueryValues[digitalSign]))
                    {
                        sSESSIONKEY = QueryValues[sessionKey];
                        sXMLRES = QueryValues[xmlRes];
                        sDIGITALSIGN = QueryValues[digitalSign];
                    }
                    else
                    {
                        sSESSIONKEY = PostedValues[sessionKey];
                        sXMLRES = PostedValues[xmlRes];
                        sDIGITALSIGN = PostedValues[digitalSign];

                        //LoggerHelper.Error(sSESSIONKEY + "," + sXMLRES + "," + sDIGITALSIGN,"GetKeysFromProdubancoGatewayResponse");
                    }

                    var oVPOSBean = new VPOSBean();

                    oVPOSBean.cipheredXML = sXMLRES;
                    oVPOSBean.cipheredSessionKey = sSESSIONKEY;
                    oVPOSBean.cipheredSignature = sDIGITALSIGN;

                    string vectorId; // = GetConfigEntry("paymentGatewayVectorId"); //vectorId

                    string R1;
                        // = ResolveUrl("\\App_data\\PaymentsKeysProdubanco\\paymentGatewayVPOSSignaturePublicKey.txt");
                    string R2;
                        // = ResolveUrl("\\App_data\\PaymentsKeysProdubanco\\paymentGatewayHerbalifeCRYPTOPrivateKey.txt");
                    if (RootUrl.Contains("local") || RootUrl.Contains("qa") || RootUrl.Contains("uat"))
                    {
                        vectorId = GetConfigEntry("paymentGatewayVectorId");
                        R1 = PaymentGatewayInvoker.ResolveUrl("\\App_data\\PaymentsKeysProdubanco\\paymentGatewayVPOSSignaturePublicKey.txt");
                        R2 =
                            PaymentGatewayInvoker.ResolveUrl("\\App_data\\PaymentsKeysProdubanco\\paymentGatewayHerbalifeCRYPTOPrivateKey.txt");
                    }
                    else
                    {
                        vectorId = GetConfigEntry("paymentGatewayBetaVectorId");
                        R1 =
                            PaymentGatewayInvoker.ResolveUrl(
                                "\\App_data\\PaymentsKeysProdubanco\\BetapaymentGatewayVPOSSignaturePublicKey.txt");
                        R2 =
                            PaymentGatewayInvoker.ResolveUrl(
                                "\\App_data\\PaymentsKeysProdubanco\\BetapaymentGatewayHerbalifeCRYPTOPrivateKey.txt");
                    }

                    var srVPOSLlaveCifradoPublica = new StreamReader(R1);
                    var srComercioLlaveFirmaPrivada = new StreamReader(R2);

                    var oVPOSReceive = new VPOSReceive(srVPOSLlaveCifradoPublica, srComercioLlaveFirmaPrivada, vectorId);
                    oVPOSReceive.execute(ref oVPOSBean);

                    OrderNumber = oVPOSBean.purchaseOperationNumber;

                    PaymentGatewayInvoker.LogMessage(
                        PaymentGatewayLogEntryType.Response, OrderNumber, string.Empty, GatewayName,
                        PaymentGatewayRecordStatusType.Unknown,
                        String.Format("3.- produbanco Payment Gateway Response {0} , {1} , {2}", vectorId, R1, R2));

                    PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Response, OrderNumber, string.Empty,
                                                     GatewayName, PaymentGatewayRecordStatusType.Unknown,
                                                     "Getting the OrderNumber Successfully");
                    //Validate this is not a spoof
                    if (oVPOSBean.validSign)
                    {
                        IsApproved = oVPOSBean.authorizationResult == "00";
                        PaymentGatewayInvoker.LogMessage(
                            PaymentGatewayLogEntryType.Response, OrderNumber, string.Empty, GatewayName,
                            PaymentGatewayRecordStatusType.Unknown,
                            String.Format("4.- produbanco Payment Gateway Response  is approved?= {0}  ", IsApproved));

                        if (IsApproved)
                        {
                            AuthorizationCode = oVPOSBean.authorizationCode;
                            TransactionCode = oVPOSBean.authorizationResult;

                            CardType = oVPOSBean.cardType == "VISA"
                                           ? IssuerAssociationType.Visa
                                           : IssuerAssociationType.MasterCard;
                        }
                        else
                        {
                            AuthorizationCode = oVPOSBean.errorCode;
                            TransactionCode = oVPOSBean.errorMessage;
                            Status = PaymentGatewayRecordStatusType.Declined;
                            //base.AuthResultMissing = true;
                        }

                        PaymentGatewayInvoker.LogMessage(
                            PaymentGatewayLogEntryType.Response, OrderNumber, string.Empty, GatewayName,
                            PaymentGatewayRecordStatusType.Unknown,
                            String.Format(
                                "5.- produbanco Payment Gateway Response  auth code {0} , transactioncode {1}  ",
                                AuthorizationCode, TransactionCode));
                        GatewayName = PaymentGatewayName;
                    }

                    else
                    {
                        LogSecurityWarning(GatewayName);
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
            string configEntries = Settings.GetRequiredAppSetting("ProdubancoPaymentGateWay");
            if (!string.IsNullOrEmpty(configEntries))
            {
                var allEntries = configEntries.Split(new[] {';'});
                if (null != allEntries && allEntries.Length > 0)
                {
                    foreach (string entry in allEntries)
                    {
                        var item = entry.Split(new[] {'='});
                        if (null != item && item.Length > 1)
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


        //public override void GetPaymentInfo(SerializedOrderHolder holder)
        //{
        //    OrderImportBtWS.Payment payment = holder.BTOrder.Payments[0];
        //    CreditPayment_V01 orderPayment = holder.Order.Payments[0] as CreditPayment_V01;
        //    if (!string.IsNullOrEmpty(CardNumber))
        //    {
        //        payment.AccountNumber = CardNumber;
        //    }
        //    else
        //    {
        //        payment.AccountNumber = PaymentInfoProvider.GetDummyCreditCardNumber(HL.Order.ValueObjects.PaymentTypes.IssuerAssociationType.Visa);
        //    }
        //    payment.PaymentCode = HLConfigManager.CurrentPlatformConfigs[holder.Locale].PaymentsConfiguration.PaymentGatewayPayCode;
        //    if (string.IsNullOrEmpty(payment.PaymentCode))
        //    {
        //        payment.PaymentCode = CreditCard.CardTypeToHPSCardType(CardType);
        //    }
        //    orderPayment.Card.IssuerAssociation = CreditCard.GetCardType(payment.PaymentCode);
        //    if (!string.IsNullOrEmpty(AuthorizationCode))
        //    {
        //        payment.AuthNumber = AuthorizationCode;
        //    }
        //    orderPayment.AuthorizationCode = payment.AuthNumber;
        //    if (!string.IsNullOrEmpty(TransactionCode))
        //    {
        //        payment.TransactionCode = TransactionCode;
        //    }
        //    orderPayment.TransactionID = payment.TransactionCode;
        //    //They don't always return the full cardnumber, so if less than 15, use the base method which has a fixer in it
        //    if (payment.AccountNumber.Length < 15)
        //    {
        //        payment.AccountNumber = new string('*', 16 - payment.AccountNumber.Length) + payment.AccountNumber;
        //    }
        //    orderPayment.Card.AccountNumber = payment.AccountNumber;
        //    payment.Currency = _Check.Currency;
        //    payment.ClientReferenceNumber = AuthorizationCode;
        //    payment.NumberOfInstallments = 1;
        //}
    }
}