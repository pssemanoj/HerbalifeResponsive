
using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;


namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class BrazilPaymentGateWayResponse : PaymentGatewayResponse
    {
        private const string Order = "merch_ref";
        private const string AuthResult = "id";
        private const string GateWay = "Agency";
        private const string PaymentGateWayName = "Bradesco";
        private const string AuthCode = "id";
        private const string MerchantCode = "merchant";
        private const string Merchant = "herbalife";
        private const string Bell = "Bell";
        private const string BellRequest = "BellRequest";

        public override bool CanProcess
        {
            get
            {
                String xmlbellstring = string.Empty;
                bool canProcess = false;
                HttpContext context = HttpContext.Current;
                HttpRequest request = context.Request;

                if ((QueryValues[GateWay] == PaymentGateWayName) && (QueryValues[Bell] == BellRequest))
                {
                    
                    canProcess = true;

                    if ((PostedValues[MerchantCode] == Merchant) && !string.IsNullOrEmpty(PostedValues[Order]) && !string.IsNullOrEmpty(PostedValues[AuthResult]))
                    {
                       
                        xmlbellstring = GenerateXmlBellRequest(PostedValues[MerchantCode], PostedValues[Order], PostedValues[AuthCode]);
                        

                        String probeXmlRequest = string.Empty;

                        probeXmlRequest = GenerateXmlProbeRequest(PostedValues[Order], PostedValues[AuthCode]);
                        String responseProbe = OrderProvider.SendBPagServiceRequest("1.1.0", "probe", "herbalife", "Herbalife_WS", "123456", probeXmlRequest);

                        int probeStatus ;
                        String probeMessage = string.Empty;
                        probeStatus = GetProbeXmlStatus(responseProbe);

                        probeMessage = GetProbeXmlMessage(responseProbe);

                        if (probeStatus == 0)
                        {
                            IsApproved = probeStatus == 0;//PostedValues[AuthResult] == "A";
                            //CardNumber = PostedValues[cardNumber];
                            CardType = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.IssuerAssociationType.GenericDebitCard;
                            AuthorizationCode = probeMessage;// PostedValues[AuthCode]; 
                           // SendBellPost(request.Url.ToString(), xmlbellstring);
                        }
                    }
                    else
                    {
                        base.AuthResultMissing = true;
                    }
                }
               
                OrderNumber = PostedValues[Order];
                return canProcess;
            }
        }

        public BrazilPaymentGateWayResponse()
        {
            base.GatewayName = PaymentGateWayName;
        }

        public string GenerateXmlBellRequest(string merchant, string merch_ref, string id)
        {

            #region "create Skeleton"
            // Create Skeleton
            XDocument xmlToSend = new XDocument();
            XDeclaration xDeclaration = new XDeclaration("1.0", "UTF-8", "no");
            xmlToSend.Declaration = xDeclaration;
            XElement xmlRoot = new XElement("bellReturn");
            xmlToSend.Add(xmlRoot);

            xmlRoot.Add(new XElement("status", 1));
            xmlRoot.Add(new XElement("msg", "Sucesso"));

            #endregion

            String xmlReadyToSend;
            xmlReadyToSend = xmlToSend.Declaration.ToString() + xmlToSend.Root.ToString();
            return xmlReadyToSend;

        }

        //Name:GetProbeXml
        //Description: Get xml from probe web service
        //
        private int GetProbeXmlStatus(string xmlresponse)
        {
            XDocument doc = XDocument.Parse(xmlresponse);

            #region Get the response

            #region "payOrderReturn"
            var requeststatusandmessage =
            from probeReturn in doc.Elements("probeReturn")
            select new { Status = (int)probeReturn.Element("status"), Message = (String)probeReturn.Element("msg") };
            #endregion

            #region "bpag_data"
            var requestbpag_data =
            from bpag_data in doc.Elements("probeReturn").Elements("order_data").Elements("order").Elements("bpag_data")
            select new
            {
                StatusPpag = (int)bpag_data.Element("status"),
                MessageBpag = (String)bpag_data.Element("msg"),
                Url = (String)bpag_data.Element("url"),
                IdOrderPag = (int)bpag_data.Element("id")
            };
            #endregion

            #region "fi_data"
            var requestfi_data =
            from fi_data in doc.Elements("probeReturn").Elements("order_data").Elements("order").Elements("fi_data").Elements("fi")
            select new
            {
                bpag_payment_id = (int)fi_data.Element("bpag_payment_id"),
                status = (int)fi_data.Element("status"),
                normalized_status = (int)fi_data.Element("normalized_status"),
                MessageBpag = (String)fi_data.Element("msg"),
                PaymentMethod = (String)fi_data.Element("payment_method"),
                normalized_payment_method = (String)fi_data.Element("normalized_payment_method"),
                installments = (String)fi_data.Element("installments"),
                value = (String)fi_data.Element("value"),
                original_value = (int)fi_data.Element("original_value"),
                trn_type = (int)fi_data.Element("trn_type"),
                id = (String)fi_data.Element("id"),
                date = (String)fi_data.Element("date"),  

            };

            #endregion

            #endregion
            return requestfi_data.FirstOrDefault().normalized_status;

        }

        private string GetProbeXmlMessage(string xmlresponse)
        {
            XDocument doc = XDocument.Parse(xmlresponse);

            #region Get the response

            #region "payOrderReturn"
            var requeststatusandmessage =
            from probeReturn in doc.Elements("probeReturn")
            select new { Status = (int)probeReturn.Element("status"), Message = (String)probeReturn.Element("msg") };
            #endregion

            #region "bpag_data"
            var requestbpag_data =
            from bpag_data in doc.Elements("probeReturn").Elements("order_data").Elements("order").Elements("bpag_data")
            select new
            {
                StatusPpag = (int)bpag_data.Element("status"),
                MessageBpag = (String)bpag_data.Element("msg"),
                Url = (String)bpag_data.Element("url"),
                IdOrderPag = (int)bpag_data.Element("id")
            };
            #endregion

            #region "fi_data"
            var requestfi_data =
            from fi_data in doc.Elements("probeReturn").Elements("order_data").Elements("order").Elements("fi_data").Elements("fi")
            select new
            {
                bpag_payment_id = (int)fi_data.Element("bpag_payment_id"),
                status = (int)fi_data.Element("status"),
                normalized_status = (int)fi_data.Element("normalized_status"),
                MessageBpag = (String)fi_data.Element("msg"),
                PaymentMethod = (String)fi_data.Element("payment_method"),
                normalized_payment_method = (String)fi_data.Element("normalized_payment_method"),
                installments = (String)fi_data.Element("installments"),
                value = (String)fi_data.Element("value"),
                original_value = (int)fi_data.Element("original_value"),
                trn_type = (int)fi_data.Element("trn_type"),
                id = (String)fi_data.Element("id"),
                date = (String)fi_data.Element("date"),

            };

            #endregion

            #endregion
            return requestbpag_data.FirstOrDefault().IdOrderPag.ToString();

        }


        public string GenerateXmlProbeRequest(string merch_ref, string id)
        {

            #region "create Skeleton"
            // Create Skeleton
            XDocument xmlToSend = new XDocument();
            XDeclaration xDeclaration = new XDeclaration("1.0", "UTF-8", "no");
            xmlToSend.Declaration = xDeclaration;
            XElement xmlRoot = new XElement("Probe");
            xmlToSend.Add(xmlRoot);


            xmlRoot.Add(new XElement("merch_ref", merch_ref));
            xmlRoot.Add(new XElement("id", id));

            #endregion

            String xmlReadyToSend;
            xmlReadyToSend = xmlToSend.Declaration.ToString() + xmlToSend.Root.ToString();
            return xmlReadyToSend;

        }

        public void SendBellPost(string bellUrl, string xmlBell)
        {
            HttpContext.Current.Response.Clear();
            //StringBuilder responseBell = new StringBuilder();
            //responseBell.Append("<html><head></head><body>\n");
            //responseBell.Append("<form");
            //responseBell.Append(" name=\"form\" id=\"form\"");
            //responseBell.Append("action=\"" + bellUrl + "\"");
            //responseBell.Append(">\n");
            //responseBell.Append("<input type=\"hidden\" name=\"Bell\" value=\"" +
            //     HttpContext.Current.Server.UrlEncode(xmlBell) + "\">\n");
            //responseBell.Append("</form>\n");
            //responseBell.Append("<script language=\"JavaScript\">document.form.submit();</script>\n");
            //responseBell.Append("</body></html>\n");
            //// Response.Write(responseBell.ToString());

            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
           // sb.AppendFormat("<form name='form' action='{0}' method='post' >", bellUrl);
            sb.AppendFormat("<form name='form' action='{0}' method='post' >", "http://www.informador.com.mx"); 
            sb.Append("<input type=\"hidden\" name=\"Bell\" value=\"" +
                 HttpContext.Current.Server.UrlEncode(xmlBell) + "\">\n");                 
            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");

            HttpContext.Current.Response.Write(sb);
            HttpContext.Current.Response.End();
        }
    }
}

