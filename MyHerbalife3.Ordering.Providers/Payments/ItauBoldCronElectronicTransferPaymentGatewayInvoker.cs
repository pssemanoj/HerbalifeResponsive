using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.Communication;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class ItauBoldCronElectronicTransferPaymentGatewayInvoker : PaymentGatewayInvoker
    {
        private ItauBoldCronElectronicTransferPaymentGatewayInvoker(string paymentMethod, decimal amount)
            : base("ItauBoldCronElectronicTransferPaymentGateway", paymentMethod, amount)
        {
        }

        public override void Submit()
        {
            try
            {
                SendPendingEmail();
                string returnUrlApproved = string.Concat(RootUrl, _config.PaymentGatewayReturnUrlApproved);
                string returnUrlDeclined = string.Concat(RootUrl, _config.PaymentGatewayReturnUrlDeclined);

                string merchant = _configHelper.GetConfigEntry("merchant");
                string user = _configHelper.GetConfigEntry("user");
                string password = _configHelper.GetConfigEntry("password");

                string xmlrequest;
                string dynamicUrlRequest;
                //  Create the service

                xmlrequest = GenerateXmlRequest(returnUrlApproved, returnUrlDeclined);

                //string xmlresponse = Servicio1.doService("1.1.0", "payOrder", merchant, user, password, xmlrequest);
                string xmlresponse = OrderProvider.SendBPagServiceRequest("1.1.0", "payOrder", merchant, user, password,
                                                                          xmlrequest);

                LogMessage(PaymentGatewayLogEntryType.Request, OrderNumber, _distributorId, _gatewayName,
                           PaymentGatewayRecordStatusType.Unknown, xmlresponse);
                //    string xmlresponse = OrderProvider.SendBPagServiceRequest("1.1.0", "payOrder", merchant, "Herbalife_WS", "123456", xmlrequest);

                dynamicUrlRequest = GetDynamicURL(xmlresponse);

                if (RootUrl.Contains("local") || RootUrl.Contains("qa") || RootUrl.Contains("uat"))
                {
                    ServicePointManager.ServerCertificateValidationCallback +=
                        (sender, certificate, chain, sslPolicyErrors) => true;
                }

                HttpContext.Current.Response.Clear();

                var sb = new StringBuilder();
                sb.Append("<html>");
                sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
                sb.AppendFormat("<form name='form' action='{0}' method='post'>", dynamicUrlRequest);
                sb.Append("</form>");
                sb.Append("</body>");

                sb.Append("</html>");

                string response = sb.ToString();
                LogMessage(PaymentGatewayLogEntryType.Request, OrderNumber, _distributorId, _gatewayName,
                           PaymentGatewayRecordStatusType.Unknown, response);

                HttpContext.Current.Response.Write(response);
                HttpContext.Current.Response.End();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format(
                        "Itau Electronic Transfer Payment Gateway Invoker exception for: order number {0}, DS ID {1} {2}",
                        OrderNumber, _distributorId, ex.Message));
            }
        }

        //Name:GenerateXmlRequest
        //Description:Generate the xml file for the PayOrder Request
        //
        private string GenerateXmlRequest(string returnUrlApproved, string returnUrlDeclined)
        {
            #region "create Skeleton"

            // Create Skeleton
            var xmlToSend = new XDocument();
            var xDeclaration = new XDeclaration("1.0", "UTF-8", "no");
            xmlToSend.Declaration = xDeclaration;
            var xmlRoot = new XElement("payOrder");
            xmlToSend.Add(xmlRoot);
            var order_data = new XElement("order_data");
            var behavior_data = new XElement("behavior_data");
            var payment_data = new XElement("payment_data");

            xmlRoot.Add(order_data);
            xmlRoot.Add(behavior_data);
            xmlRoot.Add(payment_data);

            #endregion

            #region "order data section"

            //Fill order data section
            order_data.Add(new XElement("merch_ref", OrderNumber));
            order_data.Add(new XElement("origin", "E-commerce"));
            order_data.Add(new XElement("currency", "BRL"));
            order_data.Add(new XElement("tax_boarding", 0));
            order_data.Add(new XElement("tax_freight", 0));
            order_data.Add(new XElement("tax_others", 0));
            order_data.Add(new XElement("discount_plus", 0));
            order_data.Add(new XElement("order_subtotal",
                                        (string.Format(getPriceFormat(_orderAmount), _orderAmount).Replace(".", ""))
                                            .Replace(",", "")));
                // Only positive values are accepted.Value without formatting.Two last digits are cents. Eg. 1234 = 12.34.
            order_data.Add(new XElement("interests_value", 0));
            order_data.Add(new XElement("order_total",
                                        (string.Format(getPriceFormat(_orderAmount), _orderAmount).Replace(".", ""))
                                            .Replace(",", "")));
            var order_items = new XElement("order_items");
            order_data.Add(order_items);

            #endregion

            #region "order items section"

            order_items.Add(new XElement("order_item", new XElement("code", "Grand Total"),
                                         new XElement("description", "Grand Total"),
                                         new XElement("units", 1),
                                         new XElement("unit_value",
                                                      (string.Format(getPriceFormat(_orderAmount), _orderAmount)
                                                             .Replace(".", "")).Replace(",", ""))));

            #endregion

            #region "behavior_data section"

            //Fill behavior_data section
            behavior_data.Add(new XElement("language", "ptbr"));
            behavior_data.Add(new XElement("url_post_bell",
                                           (string.Format("{0}?Agency=itauboldcron&Bell=bell&merch_ref={1}",
                                                          returnUrlApproved, OrderNumber))));
            behavior_data.Add(new XElement("url_redirect_success",
                                           (string.Format("{0}?Agency=itauboldcron&merchant=herbalife&merch_ref={1}",
                                                          returnUrlApproved, OrderNumber))));
            behavior_data.Add(new XElement("url_redirect_error",
                                           (string.Format("{0}?Agency=itauboldcron", returnUrlDeclined))));

            #endregion

            #region "payment_data section"

            //Fill payment_data section

            payment_data.Add(new XElement("payment", new XElement("payment_method", "itau")));

            #endregion

            //   xmlToSend.Save("C:\\XMLRequest Files\\bradesco.xml");
            String xmlReadyToSend;
            xmlReadyToSend = xmlToSend.Declaration + xmlToSend.Root.ToString();
            return xmlReadyToSend;
        }

        //Name:GetDynamicURL
        //Description: Get dynamic Url from Boldcron
        //
        private string GetDynamicURL(string xmlresponse)
        {
            var doc = XDocument.Parse(xmlresponse);

            #region Get the response

            #region "payOrderReturn"

            var requeststatusandmessage =
                from payOrderReturn in doc.Elements("payOrderReturn")
                select
                    new
                        {
                            Status = (int) payOrderReturn.Element("status"),
                            Message = (String) payOrderReturn.Element("msg")
                        };

            #endregion

            #region "bpag_data"

            var requestbpag_data =
                from bpag_data in doc.Elements("payOrderReturn").Elements("bpag_data")
                select new
                    {
                        StatusPpag = (int) bpag_data.Element("status"),
                        MessageBpag = (String) bpag_data.Element("msg"),
                        Url = (String) bpag_data.Element("url"),
                        IdOrderPag = (int) bpag_data.Element("id")
                    };

            #endregion

            #endregion

            string UrlToSend;
            UrlToSend = requestbpag_data.FirstOrDefault() != null ? requestbpag_data.FirstOrDefault().Url : string.Empty;
            return UrlToSend;
        }

        protected string getPriceFormat(decimal number)
        {
            return HLConfigManager.Configurations.PaymentsConfiguration.AllowDecimal
                       ? "{0:N2}"
                       : (number == (decimal) 0.0 ? "{0:0}" : "{0:#,###}");
        }

    }
}