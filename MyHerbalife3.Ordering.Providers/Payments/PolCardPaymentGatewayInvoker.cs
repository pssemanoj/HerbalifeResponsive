using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading;
using HL.Common.Utilities;

using System.Net;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class PolCardPaymentGatewayInvoker : PaymentGatewayInvoker
    {
        private PolCardPaymentGatewayInvoker(string paymentMethod, decimal amount)
            : base("PolCardPaymentGateway", paymentMethod, amount)
        {

        }

        private string getIP()
        {
            string sHostName = Dns.GetHostName();
            IPHostEntry ipE = Dns.GetHostEntry(sHostName);

            IPAddress[] IpA = ipE.AddressList;
            if (IpA.Length > 0)
            {
                return IpA[0].ToString();
            }
            return string.Empty;

        }
        public override void Submit()
        {
            string redirectUrl = _config.PaymentGatewayUrl;
            string mode = _configHelper.GetConfigEntry("paymentGatewayMode") == "0" ? "Y" : "N"; // Y means test, N means live
            int price = Convert.ToInt32(_orderAmount*100);
            string merchantId = _configHelper.GetConfigEntry("merchantAccount"); 
            string language = _config.PaymentGatewayStyle; // "PL";
            string ip = _config.PaymentGatewayApplicationId; // "63.192.82.30";

            MyHLShoppingCart myCart;
            SessionInfo sessionInfoMyCart = SessionInfo.GetSessionInfo(this._distributorId, this._locale);
            myCart = sessionInfoMyCart.ShoppingCart
                     ?? ShoppingCartProvider.GetShoppingCart(this._distributorId, this._locale);
            var address = myCart.DeliveryInfo.Address.Address;
            string email = (null != myCart && !string.IsNullOrEmpty(myCart.EmailAddress)) ? myCart.EmailAddress.ToString() : string.Empty;

            HttpContext.Current.Response.Clear();
            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
            sb.AppendFormat("<form name='form' action='{0}' method='post'>", redirectUrl);
            sb.AppendFormat("<input type='hidden' name='pos_id' value='{0}'>", merchantId);
            sb.AppendFormat("<input type='hidden' name='order_id' value='{0}'>", this.OrderNumber);
            sb.AppendFormat("<input type='hidden' name='amount' value='{0}'>", price);
            sb.AppendFormat("<input type='hidden' name='test' value='{0}'>", mode);
            sb.AppendFormat("<input type='hidden' name='language' value='{0}'>", language);
            sb.AppendFormat("<input type='hidden' name='client_ip' value='{0}'>", ip);
            sb.AppendFormat("<input type='hidden' name='street' value='{0}'>", address.Line1);
            sb.AppendFormat("<input type='hidden' name='street_n1' value='{0}'>", address.Line2 ?? string.Empty);
            sb.AppendFormat("<input type='hidden' name='street_n2' value='{0}'>", address.Line3 ?? string.Empty);
            sb.AppendFormat("<input type='hidden' name='phone' value='{0}'>", "");
            sb.AppendFormat("<input type='hidden' name='city' value='{0}'>", address.City);
            sb.AppendFormat("<input type='hidden' name='postcode' value='{0}'>", address.PostalCode);
            sb.AppendFormat("<input type='hidden' name='country' value='{0}'>", address.Country);
            sb.AppendFormat("<input type='hidden' name='email' value='{0}'>", email);
            // sb.AppendFormat("<input type='submit' value='Pay'>");
            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");

            string response = sb.ToString();
            PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, response);

            HttpContext.Current.Response.Write(response);
            HttpContext.Current.Response.End();
        }

        protected override void GetOrderNumber()
        {
            GenerateOrderNumberRequest_V01 request = new GenerateOrderNumberRequest_V01();
            request.Amount = _orderAmount;
            request.Country = _country;
            request.DistributorID = _distributorId;
            GenerateOrderNumberResponse_V01 response = OrderProvider.GenerateOrderNumber(request);
            if (null != response)
            {
                _orderNumber = response.OrderID;
                string orderData = _context.Session[PaymentGatewayInvoker.PaymentGateWayOrder] as string;
                _context.Session.Remove(PaymentGatewayInvoker.PaymentGateWayOrder);
                _context.Session[PaymentGatewayInvoker.PaymentGateWayOrder] = _orderNumber;
                int recordId = OrderProvider.InsertPaymentGatewayRecord(_orderNumber, _distributorId, _gatewayName, orderData, _locale);
            }
        }
    }
}
