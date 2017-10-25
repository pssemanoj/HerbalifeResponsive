using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Xml.Linq;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.Infrastructure.Extensions;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class BankSlipBoldCronPaymentGatewayInvoker : PaymentGatewayInvoker
    {
        private BankSlipBoldCronPaymentGatewayInvoker(string paymentMethod, decimal amount)
            : base("BankSlipBoldCronPaymentGateway", paymentMethod, amount)
        {
        }

        public override void Submit()
        {
            try
            {
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
                sb.Append("<SCRIPT TYPE=\"text/javascript\">");
                sb.Append("function popup(");
                sb.Append("mylink");
                sb.Append(", windowname) {");
                sb.Append(" if (! window.focus)return true;");
                sb.Append(" var href;");
                sb.Append(" if (typeof(mylink) == 'string') href=mylink;");
                sb.Append(" else");
                sb.Append(" href=mylink.href;");
                sb.Append(" window.open(href, windowname, 'width=800,height=400,scrollbars=yes,menubar=yes');");

                sb.Append(" return false;");
                sb.Append(" }");
                sb.Append(" </SCRIPT>");

                sb.AppendFormat("<body onLoad=\"popup('");
                sb.AppendFormat(dynamicUrlRequest);
                sb.Append(" ', 'ad')");
                sb.Append("\"");

                sb.AppendFormat("<form name='form' action='{0}' method='post'>", dynamicUrlRequest);
                sb.Append("</form>");

                sb.Append("</body>");
                sb.Append("</html>");

                string response = sb.ToString();

                LogMessage(PaymentGatewayLogEntryType.Request, OrderNumber, _distributorId, _gatewayName,
                           PaymentGatewayRecordStatusType.Unknown, response);

                //Task# 6818: Bankslip Deferred Payments
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
                    }
                    else
                    {
                        LoggerHelper.Error(
                            string.Format(
                                "BankSlipBoldCronPaymentGatewayInvoker, Session is null. Order Number  : {0} ",
                                OrderNumber));
                    }
                }

                HttpContext.Current.Response.Write(response);
                //HttpContext.Current.Response.End();
            }
            catch (Exception ex)
            {
                  LoggerHelper.Error(
                    string.Format(
                        "Bank Slip Bradesco Transfer Payment Gateway Invoker exception for: order number {0}, DS ID {1}  {2}",
                        OrderNumber, _distributorId, ex.Message));
            }
        }

        //Name:GenerateXmlRequest
        //Description:Generate the xml file for the PayOrder Request
        //
        private string GenerateXmlRequest(string returnUrlApproved, string returnUrlDeclined)
        {
            MyHLShoppingCart myCart;
            SessionInfo sessionInfoMyCart = SessionInfo.GetSessionInfo(this._distributorId, this._locale);
            myCart = sessionInfoMyCart.ShoppingCart
                     ?? ShoppingCartProvider.GetShoppingCart(this._distributorId, this._locale);

            try
            {
                #region "create Skeleton"

                // Create Skeleton
                decimal taxBrazil = 0.90m;
                var xmlToSend = new XDocument();
                var xDeclaration = new XDeclaration("1.0", "UTF-8", "no");
                xmlToSend.Declaration = xDeclaration;
                var xmlRoot = new XElement("payOrder");
                xmlToSend.Add(xmlRoot);
                var order_data = new XElement("order_data");
                var behavior_data = new XElement("behavior_data");
                var payment_data = new XElement("payment_data");
                var customer_data = new XElement("customer_data");

                xmlRoot.Add(order_data);
                xmlRoot.Add(behavior_data);
                xmlRoot.Add(payment_data);
                xmlRoot.Add(customer_data);

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
                                            (string.Format(getPriceFormat(_orderAmount + taxBrazil),
                                                           _orderAmount + taxBrazil).Replace(".", ""))
                                                .Replace(",", "")));
                    // Only positive values are accepted.Value without formatting.Two last digits are cents. Eg. 1234 = 12.34.
                order_data.Add(new XElement("interests_value", 0));
                order_data.Add(new XElement("order_total",
                                            (string.Format(getPriceFormat(_orderAmount + taxBrazil),
                                                           _orderAmount + taxBrazil).Replace(".", ""))
                                                .Replace(",", "")));
                var order_items = new XElement("order_items");
                order_data.Add(order_items);

                #endregion

                #region "order items section"

                order_items.Add(new XElement("order_item", new XElement("code", "Grand Total"),
                                             new XElement("description", "Grand Total"),
                                             new XElement("units", 1),
                                             new XElement("unit_value",
                                                          (string.Format(
                                                              getPriceFormat(_orderAmount + taxBrazil),
                                                              _orderAmount + taxBrazil).Replace(".", ""))
                                                              .Replace(",", ""))));

                #endregion

                #region "behavior_data section"

                //Fill behavior_data section
                behavior_data.Add(new XElement("language", "ptbr"));
                behavior_data.Add(new XElement("url_post_bell",
                                               (string.Format("{0}?Agency=bankslipboldcron&Bell=bell&merch_ref={1}",
                                                              returnUrlApproved, OrderNumber))));
                behavior_data.Add(new XElement("url_redirect_success",
                                               (string.Format(
                                                   "{0}?Agency=bankslipboldcron&merchant=herbalife&merch_ref={1}",
                                                   returnUrlApproved, OrderNumber))));
                behavior_data.Add(new XElement("url_redirect_error",
                                               (string.Format("{0}?Agency=bankslipboldcron", returnUrlDeclined))));

                #endregion

                #region "payment_data section"

                //Fill payment_data section

                payment_data.Add(new XElement("payment", new XElement("payment_method", "boleto_bradesco")));

                #endregion

                #region "Customer data section"

                //Fill customer_data section

                customer_data.Add(new XElement("customer_id", _distributorId));
                customer_data.Add(new XElement("customer_eval", 0));

                var customer_info = new XElement("customer_info");
                customer_data.Add(customer_info);

                string DistributorName = DistributorProfileModelHelper.DistributorName(DistributorProfileModel);

                //Fill customer_information inside customer_data section
                customer_info.Add(new XElement("first_name", DistributorName ?? string.Empty));
                customer_info.Add(new XElement("email", null != myCart.EmailAddress ? myCart.EmailAddress : string.Empty));

                //Fill billing_information inside customer_data section
                var billing_info = new XElement("billing_info");
                customer_data.Add(billing_info);

                billing_info.Add(new XElement("first_name", DistributorName ?? string.Empty));
                               
                billing_info.Add(new XElement("email", null != myCart.EmailAddress ? myCart.EmailAddress : string.Empty));

                //Fill shipment_info inside customer_data section

                var shipment_info = new XElement("shipment_info");
                customer_data.Add(shipment_info);

                shipment_info.Add(new XElement("first_name", DistributorName ?? string.Empty));
                shipment_info.Add(new XElement("email", null != myCart.EmailAddress ? myCart.EmailAddress : string.Empty));

                #endregion

                //   xmlToSend.Save("C:\\XMLRequest Files\\bradesco.xml");
                String xmlReadyToSend;
                xmlReadyToSend = xmlToSend.Declaration + xmlToSend.Root.ToString();
                return xmlReadyToSend;
            }
            catch (Exception)
            {
                throw;
            }
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