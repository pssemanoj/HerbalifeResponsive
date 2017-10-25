using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.Communication;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class BancodoBrazilElectronicTransferPaymentGatewayResponse : PaymentGatewayResponse
    {
        #region Constants and Fields

        private const string AuthCode = "id";

        private const string AuthResult = "id";

        private const string AuthResultPay = "PAGO";

        private const string Bell = "Bell";

        private const string BellRequest = "BellRequest";

        private const string GateWay = "Agency";

        //post

        private const string GateWayPost = "FORMA_PAGTO";

        private const string Merchant = "herbalife";

        private const string MerchantCode = "merchant";

        private const string NsuTrans = "NSU_TRANS";

        private const string Order = "merch_ref";

        private const string PaymentGateWayName = "Bancodobrazil";

        private const string PaymentGateWayNamePost = "bb_debito";

        private const string PedidoID = "PEDIDO_ID";

        private const string SuccessResult = "success";

        private const string Url = "url";

    
        #endregion

        #region Constructors and Destructors

        public BancodoBrazilElectronicTransferPaymentGatewayResponse()
        {
            GatewayName = PaymentGateWayName;
        }

        #endregion

        #region Public Properties

        public override bool CanProcess
        {
            get
            {
                String xmlbellstring = string.Empty;
                bool canProcess = false;
                _configHelper = new ConfigHelper("BancodoBrazilElectronicTransferPaymentGateway");
                if (HLConfigManager.Configurations.PaymentsConfiguration.DisableBancodoBrazil)
                {
                    canProcess = false;
                }
                else
                {
                    if ((QueryValues[GateWay] == PaymentGateWayName)
                        || (QueryValues[GateWayPost] == PaymentGateWayNamePost))
                    {
                        string merchant = _configHelper.GetConfigEntry("merchant");
                        string user = _configHelper.GetConfigEntry("user");
                        string password = _configHelper.GetConfigEntry("password");

                        canProcess = true;
                        var requestValues = new NameValueCollection();

                        //Is this the user coming back? (Order number is in the QueryString)
                        if (!string.IsNullOrEmpty(QueryValues[GateWayPost])
                            && string.IsNullOrEmpty(QueryValues[Bell])
                            && (QueryValues[GateWayPost] == PaymentGateWayNamePost))
                        {
                            requestValues = QueryValues;
                            string order = GetOrder(requestValues[Url]);
                            if (string.IsNullOrEmpty(order))
                            {
                                AuthResultMissing = true;
                            }

                            else
                            {
                                var authResult = requestValues[AuthResultPay];
                                var successResult = requestValues[SuccessResult];
                                var nsuTrans = requestValues[NsuTrans];
                                var pedidoID = requestValues[PedidoID];

                                OrderNumber = order;

                                if ((authResult == "true") && (successResult == "true")
                                    && (!string.IsNullOrEmpty(nsuTrans)) && (!string.IsNullOrEmpty(pedidoID)))
                                {
                                    var probeXmlRequest = string.Empty;
                                    probeXmlRequest = GenerateXmlProbeRequest(order, pedidoID);

                                    //  Create the service

                                    var responseProbe = OrderProvider.SendBPagServiceRequest(
                                        "1.1.0", "probe", merchant, user, password, probeXmlRequest);

                                    int probeStatus = GetProbeXmlStatus(responseProbe);

                                    if (probeStatus == 0)
                                    {
                                        IsApproved = true;
                                        if (!string.IsNullOrEmpty(requestValues[AuthResultPay]))
                                        {
                                            CanSubmitIfApproved = true;
                                            AuthorizationCode = requestValues[PedidoID];
                                            TransactionCode = requestValues[NsuTrans];
                                        }
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
                                xmlbellstring = GenerateXmlBellRequest(
                                    requestValues[MerchantCode], requestValues[Order], requestValues[AuthCode]);
                                SpecialResponse = xmlbellstring;
                            }
                        }
                        // OrderNumber = requestValues[Order];
                    }
                }
                return canProcess;
            }
        }

        #endregion

        #region Public Methods and Operators

        public string GenerateXmlBellRequest(string merchant, string merchRef, string id)
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

            var xmlReadyToSend = xmlToSend.Declaration + xmlToSend.Root.ToString();
            return xmlReadyToSend;
        }

        //Name:GetProbeXml
        //Description: Get xml from probe web service
        //

        public string GenerateXmlProbeRequest(string merchRef, string id)
        {
            #region "create Skeleton"

            // Create Skeleton
            var xmlToSend = new XDocument();
            var xDeclaration = new XDeclaration("1.0", "UTF-8", "no");
            xmlToSend.Declaration = xDeclaration;
            var xmlRoot = new XElement("Probe");
            xmlToSend.Add(xmlRoot);

            xmlRoot.Add(new XElement("merch_ref", merchRef));
            xmlRoot.Add(new XElement("id", id));

            #endregion

            var xmlReadyToSend = xmlToSend.Declaration + xmlToSend.Root.ToString();
            return xmlReadyToSend;
        }

        public string GetOrder(string Url)
        {
            return Url.Substring(Url.Length - 10, 10);
        }

        public override void GetPaymentInfo(SerializedOrderHolder holder)
        {
            var payment = holder.BTOrder.Payments[0];
            payment.PaymentCode = "BB";
            payment.NumberOfInstallments = 1;
            payment.AuthNumber = AuthorizationCode;
            payment.TransactionCode = TransactionCode;
        }

        #endregion

        #region Methods

        private string GetProbeXmlMessage(string xmlresponse)
        {
            var doc = XDocument.Parse(xmlresponse);

            #region Get the response

            #region "payOrderReturn"

            var requeststatusandmessage = from probeReturn in doc.Elements("probeReturn")
                                          select
                                              new
                                                  {
                                                      Status = (int) probeReturn.Element("status"),
                                                      Message = (String) probeReturn.Element("msg")
                                                  };

            #endregion

            #region "bpag_data"

            var requestbpagData =
                from bpag_data in
                    doc.Elements("probeReturn").Elements("order_data").Elements("order").Elements("bpag_data")
                select
                    new
                        {
                            StatusPpag = (int) bpag_data.Element("status"),
                            MessageBpag = (String) bpag_data.Element("msg"),
                            Url = (String) bpag_data.Element("url"),
                            IdOrderPag = (int) bpag_data.Element("id")
                        };

            #endregion

            #region "fi_data"

            var requestfiData =
                from fiData in
                    doc.Elements("probeReturn")
                       .Elements("order_data")
                       .Elements("order")
                       .Elements("fi_data")
                       .Elements("fi")
                select
                    new
                        {
                            bpag_payment_id = (int) fiData.Element("bpag_payment_id"),
                            status = (int) fiData.Element("status"),
                            normalized_status = (int) fiData.Element("normalized_status"),
                            MessageBpag = (String) fiData.Element("msg"),
                            PaymentMethod = (String) fiData.Element("payment_method"),
                            normalized_payment_method = (String) fiData.Element("normalized_payment_method"),
                            installments = (String) fiData.Element("installments"),
                            value = (String) fiData.Element("value"),
                            original_value = (int) fiData.Element("original_value"),
                            trn_type = (int) fiData.Element("trn_type"),
                            id = (String) fiData.Element("id"),
                            date = (String) fiData.Element("date"),
                        };

            #endregion

            #endregion

            return requestbpagData.FirstOrDefault().IdOrderPag.ToString();
        }

        private int GetProbeXmlStatus(string xmlresponse)
        {
            var doc = XDocument.Parse(xmlresponse);

            #region Get the response

            #region "payOrderReturn"

            var requeststatusandmessage = from probeReturn in doc.Elements("probeReturn")
                                          select
                                              new
                                                  {
                                                      Status = (int) probeReturn.Element("status"),
                                                      Message = (String) probeReturn.Element("msg")
                                                  };

            #endregion

            #region "bpag_data"

            var requestbpagData =
                from bpag_data in
                    doc.Elements("probeReturn").Elements("order_data").Elements("order").Elements("bpag_data")
                select
                    new
                        {
                            StatusPpag = (int) bpag_data.Element("status"),
                            MessageBpag = (String) bpag_data.Element("msg"),
                            Url = (String) bpag_data.Element("url"),
                            IdOrderPag = (int) bpag_data.Element("id")
                        };

            #endregion

            #region "fi_data"

            var requestfiData =
                from fiData in
                    doc.Elements("probeReturn")
                       .Elements("order_data")
                       .Elements("order")
                       .Elements("fi_data")
                       .Elements("fi")
                select
                    new
                        {
                            bpag_payment_id = (int) fiData.Element("bpag_payment_id"),
                            status = (int) fiData.Element("status"),
                            normalized_status = (int) fiData.Element("normalized_status"),
                            MessageBpag = (String) fiData.Element("msg"),
                            PaymentMethod = (String) fiData.Element("payment_method"),
                            normalized_payment_method = (String) fiData.Element("normalized_payment_method"),
                            installments = (String) fiData.Element("installments"),
                            value = (String) fiData.Element("value"),
                            original_value = (int) fiData.Element("original_value"),
                            trn_type = (int) fiData.Element("trn_type"),
                            id = (String) fiData.Element("id"),
                            date = (String) fiData.Element("date"),
                        };

            #endregion

            #endregion

            return null != requestbpagData.FirstOrDefault() ? requestbpagData.FirstOrDefault().StatusPpag : 1000;
        }


        #endregion
    }
}