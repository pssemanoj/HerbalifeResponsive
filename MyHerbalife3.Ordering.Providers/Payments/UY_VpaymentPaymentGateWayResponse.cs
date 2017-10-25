using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;
using VPOS20_PLUGIN;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class UY_VpaymentPaymentGateWayResponse : PaymentGatewayResponse
    {
        private const string GateWay = "Agency";
        private const string PaymentGatewayName = "vpayment";
        private const string sessionKey = "SESSIONKEY";
        private const string xmlRes = "XMLRES";
        private const string digitalSign = "DIGITALSIGN";

        protected CheckoutConfiguration _Check = HLConfigManager.Configurations.CheckoutConfiguration;
        protected PaymentsConfiguration _config = HLConfigManager.Configurations.PaymentsConfiguration;


        public UY_VpaymentPaymentGateWayResponse()
        {
            base.GatewayName = GatewayName;
        }

        public override bool CanProcess
        {
            get
            {
                _configHelper = new ConfigHelper("UY_VpaymentPaymentGateWay");
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
                    }

                    var oVposBean = new VPOSBean
                        {
                            cipheredXML = sXMLRES,
                            cipheredSessionKey = sSESSIONKEY,
                            cipheredSignature = sDIGITALSIGN
                        };

                    string vectorId;

                    string R1;
                    string R2;

                    if (RootUrl.Contains("local") || RootUrl.Contains("qa") || RootUrl.Contains("uat") || RootUrl.Contains("10.36.158.182") || RootUrl.Contains("http://10.36.173.61"))
                    {
                        vectorId = _configHelper.GetConfigEntry("paymentGatewayVectorId");
                        R1 =
                            PaymentGatewayInvoker.ResolveUrl(
                                "\\App_data\\PaymentsKeysVpaymentVisa\\paymentGatewayVpaymentVisaSignaturePublicKey.txt");
                        R2 =
                            PaymentGatewayInvoker.ResolveUrl(
                                "\\App_data\\PaymentsKeysVpaymentVisa\\paymentGatewayHerbalifeCRYPTOPrivateKey.txt");
                    }
                    else
                    {
                        vectorId = _configHelper.GetConfigEntry("paymentGatewayBetaVectorId");
                        R1 =
                            PaymentGatewayInvoker.ResolveUrl(
                                "\\App_data\\PaymentsKeysVpaymentVisa\\BetapaymentGatewayVpaymentVisaSignaturePublicKey.txt");
                        R2 =
                            PaymentGatewayInvoker.ResolveUrl(
                                "\\App_data\\PaymentsKeysVpaymentVisa\\BetapaymentGatewayHerbalifeCRYPTOPrivateKey.txt");
                    }

                    var srVposLlaveCifradoPublica = new StreamReader(R1);
                    var srComercioLlaveFirmaPrivada = new StreamReader(R2);

                    var oVposReceive = new VPOSReceive(srVposLlaveCifradoPublica, srComercioLlaveFirmaPrivada, vectorId);
                    oVposReceive.execute(ref oVposBean);

                    OrderNumber = oVposBean.purchaseOperationNumber;

                    PaymentGatewayInvoker.LogMessage(
                        PaymentGatewayLogEntryType.Response, OrderNumber, string.Empty, GatewayName,
                        PaymentGatewayRecordStatusType.Unknown,
                        String.Format("3.- Vpayment Payment Gateway Response {0} , {1} , {2}", vectorId, R1, R2));

                    PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Response, OrderNumber, string.Empty,
                                                     GatewayName, PaymentGatewayRecordStatusType.Unknown,
                                                     "Getting the OrderNumber Successfully");
                    //Validate this is not a spoof
                    if (oVposBean.validSign)
                    {
                        IsApproved = oVposBean.authorizationResult == "00";
                        PaymentGatewayInvoker.LogMessage(
                            PaymentGatewayLogEntryType.Response, OrderNumber, string.Empty, GatewayName,
                            PaymentGatewayRecordStatusType.Unknown,
                            String.Format("4.- Vpayment Payment Gateway Response  is approved?= {0}  ", IsApproved));

                        if (IsApproved)
                        {
                            AuthorizationCode = oVposBean.authorizationCode;
                            TransactionCode = oVposBean.authorizationResult;

                            CardType = oVposBean.cardType == "VISA"
                                           ? IssuerAssociationType.Visa
                                           : IssuerAssociationType.Visa;
                        }
                        else
                        {
                            AuthorizationCode = oVposBean.errorCode;
                            TransactionCode = oVposBean.errorMessage;
                            Status = PaymentGatewayRecordStatusType.Declined;
                            //base.AuthResultMissing = true;
                        }

                        PaymentGatewayInvoker.LogMessage(
                            PaymentGatewayLogEntryType.Response, OrderNumber, string.Empty, GatewayName,
                            PaymentGatewayRecordStatusType.Unknown,
                            String.Format(
                                "5.- Vpayment Payment Gateway Response  auth code {0} , transactioncode {1}  ",
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

    }
}