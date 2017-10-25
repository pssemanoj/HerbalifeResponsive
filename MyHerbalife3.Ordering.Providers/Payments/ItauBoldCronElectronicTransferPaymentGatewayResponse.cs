using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Web;
using System.Xml.Linq;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.Communication;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class ItauBoldCronElectronicTransferPaymentGatewayResponse : PaymentGatewayResponse
    {
        private const string Order = "merch_ref";

        private const string GateWay = "Agency";
        private const string PaymentGateWayName = "itauboldcron";
        private const string AuthCode = "id";
        private const string MerchantCode = "merchant";
        //bell
        private const string Bell = "Bell";

        //post
        private const string AuthResultPay = "PAGO";
        private const string SuccessResult = "success";
        private const string Nsu_trans = "NSU_TRANS";
        private const string Pedido_id = "PEDIDO_ID";
        private const string GateWayPost = "FORMA_PAGTO";
        private const string GateWayPostName = "itau";
        private const string Url = "url";
        private Dictionary<string, string> _configEntries;

        public ItauBoldCronElectronicTransferPaymentGatewayResponse()
        {
            base.GatewayName = GatewayName;
        }

        public override bool CanProcess
        {
            get
            {
                String xmlbellstring = string.Empty;
                bool canProcess = false;
                var context = HttpContext.Current;
                var request = context.Request;
                if (HLConfigManager.Configurations.PaymentsConfiguration.DisableItau)
                {
                    canProcess = false;
                }
                else
                {
                    if ((QueryValues[GateWay] == PaymentGateWayName) || (QueryValues[GateWayPost] == GateWayPostName))
                    {
                        string merchant = GetConfigEntry("merchant");
                        string user = GetConfigEntry("user");
                        string password = GetConfigEntry("password");

                        canProcess = true;
                        var requestValues = new NameValueCollection();

                        //Is this the user coming back? (Order number is in the QueryString)
                        if (!string.IsNullOrEmpty(QueryValues[GateWayPost]) && string.IsNullOrEmpty(QueryValues[Bell]) &&
                            (QueryValues[GateWayPost] == GateWayPostName))
                        {
                            requestValues = QueryValues;
                            string order = GetOrder(requestValues[Url]);
                            if (string.IsNullOrEmpty(order))
                            {
                                AuthResultMissing = true;
                            }

                            else
                            {
                                string authResult = requestValues[AuthResultPay];
                                string successResult = requestValues[SuccessResult];
                                string nsu_trans = requestValues[Nsu_trans];
                                string pedido_id = requestValues[Pedido_id];

                                OrderNumber = order;

                                if ((authResult == "true") && (successResult == "true") &&
                                    (!string.IsNullOrEmpty(pedido_id)))
                                {
                                    String probeXmlRequest = string.Empty;
                                    probeXmlRequest = GenerateXmlProbeRequest(order, pedido_id);

                                    //  Create the service

                                    String responseProbe = OrderProvider.SendBPagServiceRequest("1.1.0", "probe",
                                                                                                merchant, user, password,
                                                                                                probeXmlRequest);

                                    int probeStatus;
                                    probeStatus = GetProbeXmlStatus(responseProbe);

                                    if (probeStatus == 0)
                                    {
                                        IsApproved = true;
                                        CanSubmitIfApproved = true;
                                        AuthorizationCode = requestValues[Pedido_id];
                                        TransactionCode = requestValues[Pedido_id];
                                        SendConfirmationEmail();
                                    }
                                    else
                                    {
                                        SendDeclinedEmail();
                                    }
                                }
                            }

                            return canProcess;
                        }
                        else
                        {
                            //It's a blind POST from the gateway
                            requestValues = QueryValues;
                            if (string.IsNullOrEmpty(requestValues[Order]))
                            {
                                AuthResultMissing = true;
                            }
                            else
                            {
                                xmlbellstring = GenerateXmlBellRequest(requestValues[MerchantCode], requestValues[Order],
                                                                       requestValues[AuthCode]);
                                SpecialResponse = xmlbellstring;
                            }
                        }
                        //OrderNumber = requestValues[Order];
                    }
                }
                return canProcess;
            }
        }

        public string GenerateXmlBellRequest(string merchant, string merch_ref, string id)
        {
            #region "create Skeleton"

            // Create Skeleton
            var xmlToSend = new XDocument();
            var xDeclaration = new XDeclaration("1.0", "UTF-8", "no");
            xmlToSend.Declaration = xDeclaration;
            var xmlRoot = new XElement("bellReturn");
            xmlToSend.Add(xmlRoot);

            xmlRoot.Add(new XElement("status", 1));
            xmlRoot.Add(new XElement("msg", "Sucesso"));

            #endregion

            String xmlReadyToSend;
            xmlReadyToSend = xmlToSend.Declaration + xmlToSend.Root.ToString();
            return xmlReadyToSend;
        }

        //Name:GetProbeXml
        //Description: Get xml from probe web service
        //
        private int GetProbeXmlStatus(string xmlresponse)
        {
            var doc = XDocument.Parse(xmlresponse);

            #region Get the response

            #region "payOrderReturn"

            var requeststatusandmessage =
                from probeReturn in doc.Elements("probeReturn")
                select new {Status = (int) probeReturn.Element("status"), Message = (String) probeReturn.Element("msg")};

            #endregion

            #region "bpag_data"

            var requestbpag_data =
                from bpag_data in
                    doc.Elements("probeReturn").Elements("order_data").Elements("order").Elements("bpag_data")
                select new
                    {
                        StatusPpag = (int) bpag_data.Element("status"),
                        MessageBpag = (String) bpag_data.Element("msg"),
                        Url = (String) bpag_data.Element("url"),
                        IdOrderPag = (int) bpag_data.Element("id")
                    };

            #endregion

            #region "fi_data"

            var requestfi_data =
                from fi_data in
                    doc.Elements("probeReturn")
                       .Elements("order_data")
                       .Elements("order")
                       .Elements("fi_data")
                       .Elements("fi")
                select new
                    {
                        bpag_payment_id = (int) fi_data.Element("bpag_payment_id"),
                        status = (int) fi_data.Element("status"),
                        normalized_status = (int) fi_data.Element("normalized_status"),
                        MessageBpag = (String) fi_data.Element("msg"),
                        PaymentMethod = (String) fi_data.Element("payment_method"),
                        normalized_payment_method = (String) fi_data.Element("normalized_payment_method"),
                        installments = (String) fi_data.Element("installments"),
                        value = (String) fi_data.Element("value"),
                        original_value = (int) fi_data.Element("original_value"),
                        trn_type = (int) fi_data.Element("trn_type"),
                        id = (String) fi_data.Element("id"),
                        date = (String) fi_data.Element("date"),
                    };

            #endregion

            #endregion

            return null != requestbpag_data.FirstOrDefault() ? requestbpag_data.FirstOrDefault().StatusPpag : 1000;
        }

        private string GetProbeXmlMessage(string xmlresponse)
        {
            var doc = XDocument.Parse(xmlresponse);

            #region Get the response

            #region "payOrderReturn"

            var requeststatusandmessage =
                from probeReturn in doc.Elements("probeReturn")
                select new {Status = (int) probeReturn.Element("status"), Message = (String) probeReturn.Element("msg")};

            #endregion

            #region "bpag_data"

            var requestbpag_data =
                from bpag_data in
                    doc.Elements("probeReturn").Elements("order_data").Elements("order").Elements("bpag_data")
                select new
                    {
                        StatusPpag = (int) bpag_data.Element("status"),
                        MessageBpag = (String) bpag_data.Element("msg"),
                        Url = (String) bpag_data.Element("url"),
                        IdOrderPag = (int) bpag_data.Element("id")
                    };

            #endregion

            #region "fi_data"

            var requestfi_data =
                from fi_data in
                    doc.Elements("probeReturn")
                       .Elements("order_data")
                       .Elements("order")
                       .Elements("fi_data")
                       .Elements("fi")
                select new
                    {
                        bpag_payment_id = (int) fi_data.Element("bpag_payment_id"),
                        status = (int) fi_data.Element("status"),
                        normalized_status = (int) fi_data.Element("normalized_status"),
                        MessageBpag = (String) fi_data.Element("msg"),
                        PaymentMethod = (String) fi_data.Element("payment_method"),
                        normalized_payment_method = (String) fi_data.Element("normalized_payment_method"),
                        installments = (String) fi_data.Element("installments"),
                        value = (String) fi_data.Element("value"),
                        original_value = (int) fi_data.Element("original_value"),
                        trn_type = (int) fi_data.Element("trn_type"),
                        id = (String) fi_data.Element("id"),
                        date = (String) fi_data.Element("date"),
                    };

            #endregion

            #endregion

            return requestbpag_data.FirstOrDefault().IdOrderPag.ToString();
        }

        public string GenerateXmlProbeRequest(string merch_ref, string id)
        {
            #region "create Skeleton"

            // Create Skeleton
            var xmlToSend = new XDocument();
            var xDeclaration = new XDeclaration("1.0", "UTF-8", "no");
            xmlToSend.Declaration = xDeclaration;
            var xmlRoot = new XElement("Probe");
            xmlToSend.Add(xmlRoot);

            xmlRoot.Add(new XElement("merch_ref", merch_ref));
            xmlRoot.Add(new XElement("id", id));

            #endregion

            String xmlReadyToSend;
            xmlReadyToSend = xmlToSend.Declaration + xmlToSend.Root.ToString();
            return xmlReadyToSend;
        }

        public override void GetPaymentInfo(SerializedOrderHolder holder)
        {
            var payment = holder.BTOrder.Payments[0];
            var orderPayment = (holder.Order as Order_V01).Payments[0] as WirePayment_V01;

            payment.PaymentCode = "ET";
            payment.NumberOfInstallments = 1;
            payment.AuthNumber = AuthorizationCode;
            payment.TransactionCode = TransactionCode;
        }

        public string GetOrder(string Url)
        {
            return Url.Substring(Url.Length - 10, 10);
        }

        private string GetConfigEntry(string entryName)
        {
            _configEntries = new Dictionary<string, string>();
            string configEntries = Settings.GetRequiredAppSetting("ItauBoldCronElectronicTransferPaymentGateway");
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