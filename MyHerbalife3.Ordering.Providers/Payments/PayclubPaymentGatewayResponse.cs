using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Web;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;
using PlugInClient;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class PayclubPaymentGatewayResponse : PaymentGatewayResponse
    {
        private const string GateWay = "Agency";
        private const string PaymentGatewayName = "Payclub";
        private const string postprocess = "Posproceso";
        private const string ApprovedResult = "tipo=P";

        protected CheckoutConfiguration _Check = HLConfigManager.Configurations.CheckoutConfiguration;
        protected PaymentsConfiguration _config = HLConfigManager.Configurations.PaymentsConfiguration;
   
        public PayclubPaymentGatewayResponse()
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
                    _configHelper = new ConfigHelper("PayclubPaymentGateWay");
                    string vector;// = "mV6VoYVJ54A="; //this.GetConfigEntry("paymentGatewayApplicationvectorId"); //vectorId
                    string payClubPublicKey;// = ResolveUrl("\\App_data\\PaymentKeysPayclub\\PUBLICA_FIRMA_INTERDIN.pem");//paymentGatewayVPOSPublicKey.txt");
                    string herbalifePrivateKey;// = ResolveUrl("\\App_data\\PaymentKeysPayclub\\PRIVADA_CIFRADO_HERBALIFE.pem ");// paymentGatewayHerbalifePrivateKey.txt");

                    if (RootUrl.Contains("local") || RootUrl.Contains("qa") || RootUrl.Contains("uat"))
                    {
                        vector = "mV6VoYVJ54A="; //this.GetConfigEntry("paymentGatewayApplicationvectorId"); //vectorId
                        payClubPublicKey = PaymentGatewayInvoker.ResolveUrl("\\App_data\\PaymentKeysPayclub\\PUBLICA_FIRMA_INTERDIN.pem");//paymentGatewayVPOSPublicKey.txt");
                        herbalifePrivateKey = PaymentGatewayInvoker.ResolveUrl("\\App_data\\PaymentKeysPayclub\\PRIVADA_CIFRADO_HERBALIFE.pem");// paymentGatewayHerbalifePrivateKey.txt");
                    }
                    else
                    {
                        vector = "6pukFwMTOIA="; //this.GetConfigEntry("paymentGatewayApplicationvectorId"); //vectorId
                        payClubPublicKey = PaymentGatewayInvoker.ResolveUrl("\\App_data\\PaymentKeysPayclub\\BETA_PUBLICA_FIRMA_INTERDIN.pem");//paymentGatewayVPOSPublicKey.txt");
                        herbalifePrivateKey = PaymentGatewayInvoker.ResolveUrl("\\App_data\\PaymentKeysPayclub\\BETA_PRIVADA_CIFRADO_HERBALIFE.pem");// paymentGatewayHerbalifePrivateKey.txt");
                    }
                    if (QueryValues[postprocess] == "1")
                    {
                        this.CanSubmitIfApproved = false;
                        string simetricKey = _configHelper.GetConfigEntry("paymentGatewaySimetricKey");
                        string txtprocess;
                        String lsdata = QueryValues["xmlReq"];
                        String[] lsdatos = lsdata.Split(',');
                        lsdata = lsdatos[0];
                        try
                        {
                            lsdata = TripleDESEncryption.decrypt(lsdata, simetricKey, vector);
                            lsdatos = lsdata.Split('&');
                            //lsdatos[10] = "000x4H00100051";
                            PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Response, lsdatos[10].Substring(4), string.Empty, "PayclubPaymentGateWay", PaymentGatewayRecordStatusType.Unknown, String.Format("5.-Payclub Payment Gateway Response Beta generating csv line {0} , {1} , {2} , {3} , {4} , {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}", lsdatos[0], lsdatos[1], lsdatos[2], lsdatos[3], lsdatos[4], lsdatos[5], lsdatos[6], lsdatos[7], lsdatos[8], lsdatos[9], lsdatos[10], lsdatos[11], lsdatos[12]));
                            this.generateDataCSVFile(lsdatos);
                            //log
                            PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Response, lsdatos[10].Substring(4), string.Empty, "PayclubPaymentGateWay", PaymentGatewayRecordStatusType.Unknown, String.Format("6.- Payclub Payment Gateway Response getting the type of pos process lsdata[12]= {0}", lsdatos[12]));

                            if (lsdatos[12].Contains(ApprovedResult))
                            {
                                IsApproved = true;
                                //AuthorizationCode = lsdatos[1].Substring(4);
                                OrderNumber = lsdatos[10].Substring(4);
                                CardType = IssuerAssociationType.Diners;
                                txtprocess = "ESTADO=OK";
                                PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Response, lsdatos[10].Substring(4), string.Empty, "PayclubPaymentGateWay",
                                    PaymentGatewayRecordStatusType.Approved,
                                    String.Format("6 P.-DATA Payclub Payment Gateway Response getting the type of pos process P {0} , {1} , {2} , {3} , {4} ", IsApproved, AuthorizationCode, OrderNumber, GatewayName, txtprocess));
                            }
                            else
                            {
                                PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Response, lsdatos[10].Substring(4), string.Empty, "PayclubPaymentGateWay", PaymentGatewayRecordStatusType.Unknown, String.Format("6 R.- Payclub Payment Gateway Response getting the type of pos process R, setting IsApproved = false"));
                                IsApproved = false;
                                txtprocess = "ESTADO=KO";
                            }
                        }
                        catch (Exception ex)
                        {
                            IsApproved = false;
                            txtprocess = "ESTADO=KO2";
                            PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Response, lsdatos[10].Substring(4), string.Empty, "PayclubPaymentGateWay", PaymentGatewayRecordStatusType.Unknown, String.Format("8.- Payclub Payment Gateway Response getting the type of pos process exception={0}", ex.Message));
                        }
                        // PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Response, lsdatos[10].Substring(4), string.Empty, "PayclubPaymentGateWay", PaymentGatewayRecordStatusType.Unknown, String.Format("8.- Payclub Payment Gateway Response getting the type of pos process Returning special response={0}", txtprocess));
                        this.SpecialResponse = txtprocess;
                    }
                    else
                    {
                        this.CanSubmitIfApproved = true;
                        if (!string.IsNullOrEmpty(PostedValues["XMLGENERATEKEY"]) && !string.IsNullOrEmpty(PostedValues["XMLRESPONSE"]) && !string.IsNullOrEmpty(PostedValues["XMLDIGITALSIGN"]))
                        {
                            bool firmaCorrecta;
                            string xmlGenerateKey = PostedValues["XMLGENERATEKEY"];

                            PlugInClientRecive pluginr = new PlugInClientRecive();

                            pluginr.setIV(vector);
                            pluginr.setSignPublicKey(RSAEncryption.readFile(payClubPublicKey));
                            pluginr.setCipherPrivateKeyFromFile(herbalifePrivateKey);
                            pluginr.setXMLGENERATEKEY(xmlGenerateKey);
                            string cadeEnc = PostedValues["XMLRESPONSE"];
                            firmaCorrecta = pluginr.XMLProcess(cadeEnc, PostedValues["XMLDIGITALSIGN"]);
                            if (firmaCorrecta == true)
                            {
                                this.IsReturning = true;
                                string xmlResponse = pluginr.getXMLRESPONSE();
                                OrderNumber = pluginr.getTransacctionID();
                                GatewayName = PaymentGatewayName;

                                if (pluginr.getAuthorizationState() == "Y")
                                {
                                    this.Status = OrderProvider.GetPaymentGatewayRecordStatus(OrderNumber);
                                    if (RootUrl.Contains("local") || RootUrl.Contains("qa") || RootUrl.Contains("uat"))
                                    {
                                        this.IsApproved = (this.Status == PaymentGatewayRecordStatusType.Approved || this.Status == PaymentGatewayRecordStatusType.OrderSubmitted || this.Status == PaymentGatewayRecordStatusType.Unknown);
                                    }
                                    else
                                    {
                                        this.IsApproved = (this.Status == PaymentGatewayRecordStatusType.Approved || this.Status == PaymentGatewayRecordStatusType.OrderSubmitted);
                                    }
                                    if (IsApproved)
                                        PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Response, this.OrderNumber, string.Empty, "PayclubPaymentGateWay", PaymentGatewayRecordStatusType.Approved, String.Format("9.- Payclub Payment Gateway Response getting the REDIRECt getAuthorizationState= {0}", xmlResponse));

                                    CardType = IssuerAssociationType.Diners;
                                    AuthorizationCode = pluginr.getAuthorizationCode();// AUTHORIZATIONCODE;
                                    //TransactionCode = pluginr.getAuthorizationCode();//.AUTHORIZATIONCODE;

                                    if (string.IsNullOrEmpty(AuthorizationCode))
                                    {
                                        base.AuthResultMissing = true;
                                        PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Response, this.OrderNumber, string.Empty, "PayclubPaymentGateWay", PaymentGatewayRecordStatusType.Unknown, String.Format("10.- Payclub Payment Gateway Response getting the REDIRECt AUTH RESULT MISSING "));
                                    }
                                }
                                else
                                {
                                    IsApproved = false;
                                    this.Status = PaymentGatewayRecordStatusType.Declined;
                                    AuthorizationCode = pluginr.getErrorCode();
                                    TransactionCode = pluginr.getErrorDetails();
                                    PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Response, this.OrderNumber, string.Empty, "PayclubPaymentGateWay", PaymentGatewayRecordStatusType.Unknown, String.Format("XX.- Payclub Payment Gateway Response getting the REDIRECt Errors: {0}  , {1}", TransactionCode, AuthorizationCode));
                                }
                            }
                            else
                            {
                                LogSecurityWarning(this.GatewayName);
                            }
                        }
                        else
                        {
                            canProcess = false;
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

  

        private void generateDataCSVFile(string[] datos)
        {
            if (null != datos)
            {
                var sb = new StringBuilder();
                //fecha
                sb.Append(DateTime.Now.ToString("yyyyMMdd"));
                sb.Append(",");
                //hora
                sb.Append(DateTime.Now.ToLongTimeString());
                sb.Append(",");
                //referencia
                sb.Append(datos[10]);
                sb.Append(",");
                //ttar
                sb.Append(datos[4]);
                sb.Append(",");
                //subtotal
                sb.Append(datos[5]);
                sb.Append(",");
                //iva
                sb.Append(datos[6]);
                sb.Append(",");
                //ice
                sb.Append(datos[7]);
                sb.Append(",");
                //intereses
                sb.Append(datos[8]);
                sb.Append(",");
                //total
                sb.Append(datos[9]);
                sb.Append(",");
                //autorizacion
                sb.Append(datos[1]);
                sb.Append(",");
                //RUC
                sb.Append("RUC");
                sb.Append(",");
                //TipoDEcredito
                sb.Append(datos[2]);
                sb.Append(",");
                //Meses
                sb.Append(datos[3]);
                sb.Append(",");
                //Estado P o R
                sb.Append(datos[12].Substring(5));
                sb.Append(",");
                //status --vacio
                sb.Append(string.Empty);
                sb.Append(",");
                //informacion referencial

                string dataCSV = sb.ToString();
                PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Response, datos[10].Substring(4),
                                                 string.Empty, GatewayName, PaymentGatewayRecordStatusType.Unknown,
                                                 dataCSV);
            }
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

        protected override bool DetermineSubmitStatus()
        {
            //POST and Redirect are supposedly ALWAYS POST first, then client Redirect
            if (IsReturning)
            {
                //This is a Client Redirect
                return true;
            }
            else
            {
                //This is a Server Post
                return false;
            }
        }
    }
}