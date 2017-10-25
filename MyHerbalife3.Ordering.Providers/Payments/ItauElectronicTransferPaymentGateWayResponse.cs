using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Web;
using System.Xml;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class ItauElectronicTransferPaymentGatewayResponse : PaymentGatewayResponse
    {
        private const string Order = "Order";
        private const string AuthResult = "Status";

        private const string GateWay = "Agency";
        private const string PaymentGateWayName = "Itau";
        private const string Xml = "XML";
        private Dictionary<string, string> _configEntries;
        private string transactionStatus = string.Empty;

        public ItauElectronicTransferPaymentGatewayResponse()
        {
            base.GatewayName = PaymentGateWayName;
        }

        public override bool CanProcess
        {
            get
            {
                bool canProcess = false;

                var context = HttpContext.Current;
                var request = context.Request;

                //To disable Recieve Itau BR payment Responses  
                if (HLConfigManager.Configurations.PaymentsConfiguration.DisableItau)
                {
                    canProcess = false;
                }
                else
                {
                    ReloadShoppingCart = true;
                    if (QueryValues[GateWay] == PaymentGateWayName)
                    {
                        canProcess = true;
                        var requestValues = new NameValueCollection();

                        //Is this the user coming back? (Order number is in the QueryString)
                        if (!string.IsNullOrEmpty(QueryValues[Order]) && !string.IsNullOrEmpty(QueryValues[AuthResult]))
                        {
                            requestValues = QueryValues;
                            OrderNumber = requestValues[Order];
                            IsReturning = true;
                            CanSubmitIfApproved = false;
                            Status = OrderProvider.GetPaymentGatewayRecordStatus(OrderNumber);
                            IsApproved = ((Status == PaymentGatewayRecordStatusType.Approved ||
                                           Status == PaymentGatewayRecordStatusType.OrderSubmitted) ||
                                          (requestValues[AuthResult] == "1"));

                            return canProcess;
                        }
                        else
                        {
                            //It's a blind POST from the gateway
                            var xmlResponse = request.Form["XML"];
                            requestValues = PostedValues;
                            if (string.IsNullOrEmpty(PostedValues[Xml]))
                            {
                                LogSecurityWarning(GatewayName);
                                return true;
                            }
                            else
                            {
                                GetXmlValues(PostedValues[Xml]);
                                if (transactionStatus != "1")
                                {
                                    AuthResultMissing = true;
                                    IsApproved = false;
                                        // 1 means approved, 5 means processing, other mean declined                         
                                    AuthorizationCode = "";
                                    TransactionCode = "";
                                    CanSubmitIfApproved = false;
                                }
                            }
                        }
                    }
                }

                return canProcess;
            }
        }

        public override void GetPaymentInfo(SerializedOrderHolder holder)
        {
            var payment = holder.BTOrder.Payments[0];
            var orderPayment = (holder.Order as Order_V01).Payments[0] as WirePayment_V01;

            payment.PaymentCode = "ET";
            //  orderPayment.PaymentCode = "W2";

            // Need this? what other data?
            payment.NumberOfInstallments = 1;
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

        private void GetXmlValues(string xml)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(xml);

                var nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("myNS", "http://www2.superpag.com.br/Schemas");

                var nodeEstabelecimentoComercial = doc.SelectSingleNode("//myNS:EstabelecimentoComercial", nsmgr);
                var nodeOrder = doc.SelectSingleNode("//myNS:EstabelecimentoComercial//myNS:OrdemPagamento", nsmgr);
                var nodePayment =
                    doc.SelectSingleNode("//myNS:EstabelecimentoComercial//myNS:OrdemPagamento//myNS:Pagamento", nsmgr);
                var nodePaymentDetails =
                    doc.SelectSingleNode(
                        "//myNS:EstabelecimentoComercial//myNS:OrdemPagamento//myNS:Pagamento//myNS:DetalhePagamento",
                        nsmgr);

                transactionStatus = nodePaymentDetails.Attributes["Status"].Value;
                OrderNumber = nodeOrder.Attributes["Codigo"].Value;
                OrderNumber = OrderNumber.Substring(0, 10);
                string keyXml = nodeEstabelecimentoComercial.Attributes["SenhaAutenticacao"].Value;
                string passwordXml = nodeEstabelecimentoComercial.Attributes["ChaveAutenticacao"].Value;
                string key = GetConfigEntry("paymentGatewayEncryptionKey");
                string password = GetConfigEntry("paymentGatewayPassword");

                if ((key == keyXml) && (password == passwordXml))
                {
                    IsApproved = (nodePaymentDetails.Attributes["Status"].Value == "1");
                        // 1 means approved, 5 means processing, other mean declined 
                    IsCancelled = (nodePaymentDetails.Attributes["Status"].Value == "3");
                    IsPendingTransaction = (nodePaymentDetails.Attributes["Status"].Value == "5");
                    AuthorizationCode = nodePaymentDetails.Attributes["NumeroAutorizacao"].Value;
                    TransactionCode = nodePaymentDetails.Attributes["IdTransacaoIF"].Value;
                    CanSubmitIfApproved = true;
                    Status = OrderProvider.GetPaymentGatewayRecordStatus(OrderNumber);
                }
                else
                {
                    AuthResultMissing = true;
                    IsApproved = false;
                        // 1 means approved, 5 means processing, other mean declined                         
                    AuthorizationCode = "";
                    TransactionCode = "";
                    CanSubmitIfApproved = false;
                }
            }

            catch (Exception ex)
            {

                //TODO: Fix swallowed exception
                string exception = ex.ToString();
            }
        }

        private string GetConfigEntry(string entryName)
        {
            _configEntries = new Dictionary<string, string>();
            string configEntries = Settings.GetRequiredAppSetting("ItauElectronicTransferPaymentGateway");
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
    }
}