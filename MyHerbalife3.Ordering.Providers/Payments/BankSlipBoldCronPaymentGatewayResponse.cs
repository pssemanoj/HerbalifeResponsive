using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Xml.Linq;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments

{
    public class BankSlipBoldCronPaymentGatewayResponse : PaymentGatewayResponse
    {
        private const string Order = "merch_ref";
        private const string GateWay = "Agency";
        private const string PaymentGateWayName = "bankslipboldcron";
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
        private const string GateWayPostName = "boleto_bradesco";
        private const string Url = "url";
   
        public BankSlipBoldCronPaymentGatewayResponse()
        {
            base.GatewayName = GatewayName;
        }

        public override bool CanProcess
        {
            get
            {
                String xmlbellstring = string.Empty;
                bool canProcess = false;
                _configHelper = new ConfigHelper("BankSlipBoldCronPaymentGateway");
                if (HLConfigManager.Configurations.PaymentsConfiguration.DisableBankSlip)
                {
                    canProcess = false;
                }
                else
                {
                    if ((QueryValues[GateWay] == PaymentGateWayName) || (QueryValues[GateWayPost] == GateWayPostName))
                    {
                        string merchant = _configHelper.GetConfigEntry("merchant");
                        string user = _configHelper.GetConfigEntry("user");
                        string password = _configHelper.GetConfigEntry("password");

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

                                //Task# 6818: Bankslip Deferred Payments
                                if (OrderProvider.GetPaymentGatewayRecordStatus(order) ==
                                    PaymentGatewayRecordStatusType.OrderSubmitted)
                                {
                                    SessionInfo _sessionInfo = null;
                                    string _locale = HLConfigManager.Configurations.Locale;
                                    var member = ((MembershipUser<DistributorProfileModel>)Membership.GetUser()).Value;
                                    string DistributorID = (null != member) ? member.Id : string.Empty;
                                    if (!string.IsNullOrEmpty(DistributorID))
                                    {
                                        _sessionInfo = SessionInfo.GetSessionInfo(DistributorID, _locale);
                                        if (_sessionInfo != null)
                                        {
                                            _sessionInfo.OrderStatus = SubmitOrderStatus.OrderSubmitted;
                                            _sessionInfo.OrderNumber = OrderNumber;
                                            HttpContext.Current.Response.Redirect(
                                                "~/Ordering/Confirm.aspx?OrderNumber=" + _sessionInfo.OrderNumber);
                                            HttpContext.Current.Response.End();
                                        }
                                        else
                                        {
                                            LoggerHelper.Error(
                                                string.Format(
                                                    "BankSlipBoldCronPaymentGatewayResponse, Session is null. Order Number  : {0} ",
                                                    order));
                                        }
                                    }
                                    else
                                    {
                                        LoggerHelper.Error(
                                            string.Format(
                                                "BankSlipBoldCronPaymentGatewayResponse, DistributorID is null. Order Number  : {0} ",
                                                order));
                                    }
                                }

                                if ((authResult == "true") && (successResult == "true") &&
                                    (!string.IsNullOrEmpty(nsu_trans)) && (!string.IsNullOrEmpty(pedido_id)))
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
                                        TransactionCode = requestValues[Nsu_trans];
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

            payment.PaymentCode = "BT";
            payment.NumberOfInstallments = 1;
            payment.AuthNumber = AuthorizationCode;
            payment.TransactionCode = TransactionCode;
        }

        public string GetOrder(string Url)
        {
            return Url.Substring(Url.Length - 10, 10);
        }

   
    }
}